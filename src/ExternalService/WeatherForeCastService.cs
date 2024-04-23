using System.Text.Json;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;
using WeatherForecastApiWithPollyDistributedCaching.Model;
using static WeatherForecastApiWithPollyDistributedCaching.Constants.PollyRegistry;

namespace WeatherForecastApiWithPollyDistributedCaching.ExternalService;

public class WeatherForeCastService : IWeatherForeCastService
{
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<List<WeatherForecast>> _cachePolicy;
    private readonly WeatherForeCastConfig _weatherConfig;

    public WeatherForeCastService(IReadOnlyPolicyRegistry<string> policyRegistry, HttpClient httpClient, IOptionsSnapshot<WeatherForeCastConfig> config)
    {
        _httpClient = httpClient;
        _cachePolicy = policyRegistry.Get<IAsyncPolicy<List<WeatherForecast>>>(WeatherCachePolicy);
        _weatherConfig = config.Value;
    }

    public async Task<List<WeatherForecast>> GetWeather()
    {
        var requestUri = new Uri($"{_weatherConfig.BaseUri}/WeatherForecast");
        var result =
            await _cachePolicy.ExecuteAsync(async _ => await GetFromService(requestUri), new Context(requestUri.AbsoluteUri));
        return result;
    }


    private async Task<List<WeatherForecast>> GetFromService(Uri requestUri)
    {
        var result = await _httpClient.GetAsync(requestUri);

        result.EnsureSuccessStatusCode();

        var weatherForecasts = JsonSerializer.Deserialize<List<WeatherForecast>>(await result.Content.ReadAsByteArrayAsync(), new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });
        return weatherForecasts;
    }
}