using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using System.Net.Http.Headers;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddSingleton(_ =>
{
    var client = new HttpClient() { BaseAddress = new Uri("https://someApi.com") };
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("product-tool", "1.0"));
    client.DefaultRequestHeaders.Add("X-Api-Key", "some_not_very_secure_api");
    return client;
});

var app = builder.Build();

await app.RunAsync();
