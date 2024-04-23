using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using WeatherForecastApiWithPollyDistributedCaching.Model;
using WireMock.Logging;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace PollyDistributedCaching.IntegrationTests;

[Collection("WebApplicationFactory Collection")]
public class CachingApiIntegrationTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly WireMockServer _mockWeatherService;

    public CachingApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _mockWeatherService = _factory.MockWeatherService;
        _factory.ResetMocks();
        _factory.CreateClient();
    }

    [Fact]
    public async Task Calls_Service_And_Sets_The_Cache()
    {
        var weatherForecasts = new List<WeatherForecast>
        {
            new()
            {
                Summary = "Raining"
            }
        };
        _mockWeatherService.Given(Request.Create().WithPath($"/WeatherForecast").UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200).WithBody(JsonSerializer.Serialize(weatherForecasts, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })));

        var weatherResponse = await GetWeatherForecasts();
        weatherResponse.Should().BeEquivalentTo(weatherForecasts);
        var cachedResponse = await GetWeatherForecasts();
        cachedResponse.Should().BeEquivalentTo(weatherForecasts);
        MockedWeatherServiceLogEntries().Should().HaveCount(1);
    }

    [Theory]
    [InlineData(500)]
    [InlineData(400)]
    [InlineData(404)]
    public async Task DoesNot_Cache_Responses_On_Non_200_Responses(int statusCode)
    {
        _mockWeatherService.Given(Request.Create().WithPath($"/WeatherForecast").UsingGet())
             .RespondWith(
                 Response.Create()
                     .WithStatusCode(statusCode));

        var result = await _factory.Server.CreateRequest("/CachedWeatherForecast").GetAsync();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        await _factory.Server.CreateRequest("/CachedWeatherForecast").GetAsync();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        MockedWeatherServiceLogEntries().Should().HaveCount(2);
    }

    private IEnumerable<LogEntry> MockedWeatherServiceLogEntries()
    {
        return _mockWeatherService.FindLogEntries(
            Request.Create().WithPath($"/WeatherForecast").UsingGet()
        );
    }

    private async Task<List<WeatherForecast>?> GetWeatherForecasts()
    {
        var result = await _factory.Server.CreateRequest("/CachedWeatherForecast").GetAsync();
        var readAsStringAsync = await result.Content.ReadAsStringAsync();
        var weatherResponse = JsonSerializer.Deserialize<List<WeatherForecast>>(readAsStringAsync, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return weatherResponse;
    }
}