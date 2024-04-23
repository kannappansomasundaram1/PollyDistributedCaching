using WeatherForecastApiWithPollyDistributedCaching.Model;

namespace WeatherForecastApiWithPollyDistributedCaching.ExternalService;

public interface IWeatherForeCastService
{
    Task<List<WeatherForecast>> GetWeather();
}