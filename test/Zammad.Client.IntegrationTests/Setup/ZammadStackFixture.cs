using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using Microsoft.Extensions.Options;
using Testcontainers.Elasticsearch;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;

[assembly: AssemblyFixture(typeof(ZammadStackFixture))]

namespace Zammad.Client.IntegrationTests.Setup;

public class ZammadStackFixture : IAsyncLifetime
{
    private const string ZammadImage = "ghcr.io/zammad/zammad:6.5.2";
    private const string ZammadEntrypoint = "/docker-entrypoint-override";
    private const string ZammadStorage = "/opt/zammad/storage";
    private const string EntrypointFinished = "Zammad entrypoint script finished";
    private const string AutowizardFilename = "auto_wizard_integration_test.json";

    private readonly List<IAsyncDisposable> _resources = [];

    private async Task<string> GetAutowizardJson()
    {
        var json = await TestFile.ReadStringAsync("autowizard.json");
        var utf8 = Encoding.UTF8.GetBytes(json);
        var base64 = Convert.ToBase64String(utf8);
        return base64;
    }

    private readonly TaskCompletionSource _ready = new();

    public async Task WaitUntilReadyAsync() => await _ready.Task;

    private void SetReady() => _ready.TrySetResult();

    private int? _publicPort;
    private IZammadClient _client = null;

    public async Task<Uri> GetPublicUriAsync()
    {
        await WaitUntilReadyAsync();
        return GetUri(_publicPort!.Value);
    }

    public async Task<IZammadClient> GetClientAsync()
    {
        await WaitUntilReadyAsync();
        return _client!;
    }

    private static Uri GetUri(int port) => new Uri($"http://127.0.0.1:{port}");

    private static ZammadClient GetClient(Uri baseUrl)
    {
        ZammadClient client = new ZammadClient(
            new HttpClient(),
            Options.Create(
                new ZammadOptions
                {
                    BaseUrl = baseUrl,
                    Username = "admin@example.org",
                    Password = "TestPassword1234",
                }
            )
        );
        return client;
    }

    public async ValueTask InitializeAsync()
    {
        using IOutputConsumer outputConsumer = Consume.RedirectStdoutAndStderrToConsole();

        var id = Guid.NewGuid().ToString("N").Substring(0, 8);

        var environment = new Dictionary<string, string>
        {
            ["MEMCACHE_SERVERS"] = $"zammad-memcached-{id}:11211",
            ["POSTGRESQL_DB"] = "zammad_production",
            ["POSTGRESQL_HOST"] = $"zammad-postgres-{id}",
            ["POSTGRESQL_PASSWORD"] = "zammad",
            ["POSTGRESQL_USER"] = "zammad",
            ["POSTGRESQL_PORT"] = "5432",
            ["POSTGRESQL_OPTIONS"] = "?pool=10",
            ["REDIS_URL"] = $"redis://zammad-redis-{id}:6379",
            ["ELASTICSEARCH_HOST"] = $"zammad-elasticsearch-{id}",
            ["ZAMMAD_RAILSSERVER_HOST"] = $"zammad-railsserver-{id}",
            ["ZAMMAD_WEBSOCKET_HOST"] = $"zammad-websocket-{id}",
            ["AUTOWIZARD_JSON"] = await GetAutowizardJson(),
            ["AUTOWIZARD_RELATIVE_PATH"] = AutowizardFilename,
        };

        var network = new NetworkBuilder().WithName($"zammad-{id}").WithCleanUp(true).Build();
        _resources.Add(network);

        var storage = new VolumeBuilder().WithName($"zammad-{id}").WithCleanUp(true).WithReuse(false).Build();
        _resources.Add(storage);

        var zammadElasticsearch = new ElasticsearchBuilder()
            .WithImage("elasticsearch:8.19.4")
            .WithEnvironment("discovery.type", "single-node")
            .WithEnvironment("xpack.security.enabled", "false")
            .WithEnvironment("ES_JAVA_OPTS", "-Xms512m -Xmx512m")
            .WithNetwork(network)
            .WithName($"zammad-elasticsearch-{id}")
            .WithCleanUp(true)
            .Build();
        _resources.Add(zammadElasticsearch);

        var zammadPostgres = new PostgreSqlBuilder()
            .WithDatabase("zammad_production")
            .WithUsername("zammad")
            .WithPassword("zammad")
            .WithImage("postgres:17.6-alpine")
            .WithNetwork(network)
            .WithName($"zammad-postgres-{id}")
            .WithCleanUp(true)
            .WithReuse(false)
            .Build();
        _resources.Add(zammadPostgres);

        var zammadRedis = new RedisBuilder()
            .WithImage("redis:7.4.5-alpine")
            .WithNetwork(network)
            .WithName($"zammad-redis-{id}")
            .WithCleanUp(true)
            .Build();
        _resources.Add(zammadRedis);

        var zammadMemcached = new ContainerBuilder()
            .WithImage("memcached:1.6.39-alpine")
            .WithName($"zammad-memcached-{id}")
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
            .WithName($"zammad-init-{id}")
            .DependsOn(zammadPostgres)
            .WithEnvironment(environment)
            .WithCreateParameterModifier(x => x.User = "0:0")
            .WithNetwork(network)
            .WithVolumeMount(storage, ZammadStorage)
            .WithBindMount(TestFile.GetAbsolutePath("docker-entrypoint"), ZammadEntrypoint, AccessMode.ReadOnly)
            .WithEntrypoint(ZammadEntrypoint)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilMessageIsLogged(
                        $".*{EntrypointFinished}.*",
                        strategy => strategy.WithTimeout(TimeSpan.FromMinutes(5)).WithMode(WaitStrategyMode.OneShot)
                    )
            )
            .WithCleanUp(true)
            .Build();
        _resources.Add(zammadInit);

        var zammadRailsserver = new ContainerBuilder()
            .WithImage(ZammadImage)
            .WithCommand("zammad-railsserver")
            .WithName($"zammad-railsserver-{id}")
            .DependsOn(zammadMemcached)
            .DependsOn(zammadPostgres)
            .DependsOn(zammadRedis)
            .WithEnvironment(environment)
            .WithVolumeMount(storage, ZammadStorage)
            .WithNetwork(network)
            .WithCleanUp(true)
            .Build();
        _resources.Add(zammadRailsserver);

        var zammadNginx = new ContainerBuilder()
            .WithImage(ZammadImage)
            .WithCommand("zammad-nginx")
            .WithName($"zammad-nginx-{id}")
            .DependsOn(zammadRailsserver)
            .WithEnvironment(environment)
            .WithNetwork(network)
            .WithExposedPort(8080)
            .WithPortBinding(8080, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(8080))
            .WithVolumeMount(storage, ZammadStorage)
            .WithCleanUp(true)
            .Build();
        _resources.Add(zammadNginx);

        var zammadScheduler = new ContainerBuilder()
            .WithImage(ZammadImage)
            .WithCommand("zammad-scheduler")
            .WithName($"zammad-scheduler-{id}")
            .DependsOn(zammadMemcached)
            .DependsOn(zammadPostgres)
            .DependsOn(zammadRedis)
            .WithEnvironment(environment)
            .WithVolumeMount(storage, ZammadStorage)
            .WithNetwork(network)
            .WithCleanUp(true)
            .Build();
        _resources.Add(zammadScheduler);

        var zammadWebsocket = new ContainerBuilder()
            .WithImage(ZammadImage)
            .WithCommand("zammad-websocket")
            .WithName($"zammad-websocket-{id}")
            .DependsOn(zammadMemcached)
            .DependsOn(zammadPostgres)
            .DependsOn(zammadRedis)
            .WithEnvironment(environment)
            .WithVolumeMount(storage, ZammadStorage)
            .WithNetwork(network)
            .WithCleanUp(true)
            .Build();
        _resources.Add(zammadWebsocket);

        await Task.WhenAll([network.CreateAsync(), storage.CreateAsync()]);

        await Task.WhenAll([
            zammadElasticsearch.StartAsync(),
            zammadPostgres.StartAsync(),
            zammadRedis.StartAsync(),
            zammadMemcached.StartAsync(),
        ]);

        await Task.WhenAll([
            zammadInit.StartAsync(),
            zammadRailsserver.StartAsync(),
            zammadNginx.StartAsync(),
            zammadScheduler.StartAsync(),
            zammadWebsocket.StartAsync(),
        ]);

        _publicPort = zammadNginx.GetMappedPublicPort(8080);
        _client = GetClient(GetUri(_publicPort.Value));

        TimeSpan timeout = TimeSpan.FromMinutes(1);
        var start = DateTimeOffset.UtcNow;

        while (true)
        {
            try
            {
                await _client.GetUserMeAsync();
                break;
            }
            catch (Exception e)
            {
                if (DateTimeOffset.UtcNow - start > timeout)
                {
                    throw new TimeoutException("Timed out waiting for Zammad to become ready.", e);
                }
            }

            await Task.Delay(1000);
        }

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
