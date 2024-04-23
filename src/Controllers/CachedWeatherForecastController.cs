using Microsoft.AspNetCore.Mvc;
using WeatherForecastApiWithPollyDistributedCaching.ExternalService;

namespace WeatherForecastApiWithPollyDistributedCaching.Controllers;

[ApiController]
[Route("[controller]")]
public class CachedWeatherForecastController : ControllerBase
{
    private readonly IWeatherForeCastService _weatherForeCastService;
    private readonly ILogger<CachedWeatherForecastController> _logger;

    public CachedWeatherForecastController(IWeatherForeCastService weatherForeCastService, ILogger<CachedWeatherForecastController> logger)
    {
        _weatherForeCastService = weatherForeCastService;
        _logger = logger;
    }

    [HttpGet(Name = "GetCachedWeatherForecast")]
    public async Task<IActionResult> GetCachedWeatherForeCast()
    {
        var result = await _weatherForeCastService.GetWeather();
        return Ok(result);
    }
}
