using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using WeatherForecastApiWithPollyDistributedCaching;
using WireMock.Server;

namespace PollyDistributedCaching.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public WireMockServer MockWeatherService { get; }

    public CustomWebApplicationFactory()
    {
        MockWeatherService = WireMockServer.Start(3002);
    }

    public void ResetMocks()
    {
        MockWeatherService.Reset();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(b =>
        {
            b.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
        });

        var host = base.CreateHost(builder);
        return host;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IDistributedCache, MemoryDistributedCache>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        MockWeatherService.Dispose();
    }
}