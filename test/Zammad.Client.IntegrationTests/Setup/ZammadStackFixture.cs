using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using Testcontainers.Elasticsearch;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;
using Zammad.Client.IntegrationTests.Setup;

[assembly: AssemblyFixture(typeof(ZammadStackFixture))]

namespace Zammad.Client.IntegrationTests.Setup;

public class ZammadStackFixture : IAsyncLifetime
{
    private const string ZammadImage = "ghcr.io/zammad/zammad:6.5.2";

    private readonly List<IAsyncDisposable> _resources = [];

    private async Task<string> GetAutowizardJson()
    {
        var json = await TestFile.ReadStringAsync("autowizard.json");
        var utf8 = Encoding.UTF8.GetBytes(json);
        var base64 = Convert.ToBase64String(utf8);
        return base64;
    }

    private TaskCompletionSource _ready = new();

    public async Task WaitUntilReadyAsync() => await _ready.Task;

    private void SetReady() => _ready.TrySetResult();

    private int? _publicPort = null;

    public async Task<Uri> GetPublicUriAsync()
    {
        await WaitUntilReadyAsync();
        return new Uri($"http://127.0.0.1:{_publicPort.Value}");
    }

    public async ValueTask InitializeAsync()
    {
        using IOutputConsumer outputConsumer = Consume.RedirectStdoutAndStderrToConsole();

        var environment = new Dictionary<string, string>
        {
            ["MEMCACHE_SERVERS"] = "zammad-memcached:11211",
            ["POSTGRESQL_DB"] = "zammad_production",
            ["POSTGRESQL_HOST"] = "zammad-postgres",
            ["POSTGRESQL_PASSWORD"] = "zammad",
            ["POSTGRESQL_USER"] = "zammad",
            ["POSTGRESQL_PORT"] = "5432",
            ["POSTGRESQL_OPTIONS"] = "?pool=10",
            ["REDIS_URL"] = "redis://zammad-redis:6379",
            ["AUTOWIZARD_JSON"] = await GetAutowizardJson(),
        };

        var network = new NetworkBuilder().WithName(Guid.NewGuid().ToString("D")).WithCleanUp(true).Build();
        _resources.Add(network);

        var storage = new VolumeBuilder().WithName(Guid.NewGuid().ToString("D")).WithCleanUp(true).Build();
        _resources.Add(storage);

        var zammadElasticsearch = new ElasticsearchBuilder()
            .WithImage("elasticsearch:8.19.4")
            .WithEnvironment("discovery.type", "single-node")
            .WithEnvironment("xpack.security.enabled", "false")
            .WithEnvironment("ES_JAVA_OPTS", "-Xms512m -Xmx512m")
            .WithNetwork(network)
            .WithName("zammad-elasticsearch")
            .WithCleanUp(true)
            .Build();
        _resources.Add(zammadElasticsearch);

        var zammadPostgres = new PostgreSqlBuilder()
            .WithDatabase("zammad_production")
            .WithUsername("zammad")
            .WithPassword("zammad")
            .WithImage("postgres:17.6-alpine")
            .WithNetwork(network)
            .WithName("zammad-postgres")
            .WithCleanUp(true)
            .Build();
        _resources.Add(zammadPostgres);

        var zammadRedis = new RedisBuilder()
            .WithImage("redis:7.4.5-alpine")
            .WithNetwork(network)
            .WithName("zammad-redis")
            .WithCleanUp(true)
            .Build();
        _resources.Add(zammadRedis);

        var zammadMemcached = new ContainerBuilder()
            .WithImage("memcached:1.6.39-alpine")
            .WithName("zammad-memcached")
            .WithCommand("--memory-limit=64")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(11211))
            .WithNetwork(network)
            .WithCleanUp(true)
            .WithOutputConsumer(outputConsumer)
            .Build();
        _resources.Add(zammadMemcached);

        var zammadInit = new ContainerBuilder()
            .WithImage(ZammadImage)
            .WithCommand("zammad-init")
            .WithName("zammad-init")
            .DependsOn(zammadPostgres)
            .WithEnvironment(environment)
            .WithCreateParameterModifier(x => x.User = "0:0")
            .WithNetwork(network)
            .WithVolumeMount(storage, "/opt/zammad/storage")
            .WithCleanUp(true)
            .Build();
        _resources.Add(zammadInit);

        var zammadRailsserver = new ContainerBuilder()
            .WithImage(ZammadImage)
            .WithCommand("zammad-railsserver")
            .WithName("zammad-railsserver")
            .DependsOn(zammadMemcached)
            .DependsOn(zammadPostgres)
            .DependsOn(zammadRedis)
            .WithEnvironment(environment)
            .WithVolumeMount(storage, "/opt/zammad/storage")
            .WithNetwork(network)
            .WithCleanUp(true)
            .Build();
        _resources.Add(zammadRailsserver);

        var zammadNginx = new ContainerBuilder()
            .WithImage(ZammadImage)
            .WithCommand("zammad-nginx")
            .WithName("zammad-nginx")
            .DependsOn(zammadRailsserver)
            .WithEnvironment(environment)
            .WithNetwork(network)
            .WithExposedPort(8080)
            .WithPortBinding(8080, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(8080))
            .WithVolumeMount(storage, "/opt/zammad/storage")
            .WithCleanUp(true)
            .Build();
        _resources.Add(zammadNginx);

        var zammadScheduler = new ContainerBuilder()
            .WithImage(ZammadImage)
            .WithCommand("zammad-scheduler")
            .WithName("zammad-scheduler")
            .DependsOn(zammadMemcached)
            .DependsOn(zammadPostgres)
            .DependsOn(zammadRedis)
            .WithEnvironment(environment)
            .WithVolumeMount(storage, "/opt/zammad/storage")
            .WithNetwork(network)
            .WithCleanUp(true)
            .Build();
        _resources.Add(zammadScheduler);

        var zammadWebsocket = new ContainerBuilder()
            .WithImage(ZammadImage)
            .WithCommand("zammad-websocket")
            .WithName("zammad-websocket")
            .DependsOn(zammadMemcached)
            .DependsOn(zammadPostgres)
            .DependsOn(zammadRedis)
            .WithEnvironment(environment)
            .WithVolumeMount(storage, "/opt/zammad/storage")
            .WithNetwork(network)
            .WithCleanUp(true)
            .Build();
        _resources.Add(zammadWebsocket);

        await Task.WhenAll([network.CreateAsync(), storage.CreateAsync()]);

        await Task.WhenAll(
            [
                zammadElasticsearch.StartAsync(),
                zammadPostgres.StartAsync(),
                zammadRedis.StartAsync(),
                zammadMemcached.StartAsync(),
            ]
        );

        await Task.WhenAll(
            [
                zammadInit.StartAsync(),
                zammadRailsserver.StartAsync(),
                zammadNginx.StartAsync(),
                zammadScheduler.StartAsync(),
                zammadWebsocket.StartAsync(),
            ]
        );

        _publicPort = zammadNginx.GetMappedPublicPort(8080);
        SetReady();
    }

    public async ValueTask DisposeAsync()
    {
        _resources.Reverse();
        foreach (IAsyncDisposable asyncDisposable in _resources)
        {
            await asyncDisposable.DisposeAsync();
        }
    }
}
