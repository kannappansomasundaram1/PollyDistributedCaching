using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Polly;
using Polly.Caching;
using Polly.Caching.Distributed;
using Polly.Registry;
using WeatherForecastApiWithPollyDistributedCaching.ExternalService;
using WeatherForecastApiWithPollyDistributedCaching.Model;
using static WeatherForecastApiWithPollyDistributedCaching.Constants.PollyRegistry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<IWeatherForeCastService, WeatherForeCastService>();
builder.Services.Configure<WeatherForeCastConfig>(builder.Configuration.GetSection("WeatherForeCastApi"));

builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = "localhost:6379"; });

builder.Services.AddSingleton<IAsyncCacheProvider<List<WeatherForecast>>>(serviceProvider =>
    serviceProvider
        .GetRequiredService<IDistributedCache>()
        .AsAsyncCacheProvider<byte[]>()
        .WithSerializer(new DotnetSerializer<List<WeatherForecast>>(new JsonSerializerOptions()))
    );

builder.Services.AddSingleton<IReadOnlyPolicyRegistry<string>, PolicyRegistry>(serviceProvider =>
{
    var registry = new PolicyRegistry
    {
        {
            WeatherCachePolicy,
            Policy.CacheAsync(serviceProvider.GetRequiredService<IAsyncCacheProvider<List<WeatherForecast>>>(),
                TimeSpan.FromMinutes(5))
        }
    };

    return registry;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

namespace WeatherForecastApiWithPollyDistributedCaching
{
    public partial class Program
    {
    }
}