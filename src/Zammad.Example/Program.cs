using Zammad.Client.Extensions;
using Zammad.Example;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddZammadClient(builder.Configuration.GetSection("Zammad"));

var host = builder.Build();
await host.RunAsync();
