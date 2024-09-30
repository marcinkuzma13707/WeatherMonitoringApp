using WeatherMonitoringApp.Models;

namespace WeatherMonitoringApp.Services;

public interface IWeatherService
{
    Task SaveWeatherDataAsync(string city);
    Task<IEnumerable<WeatherDataDto>> GetWeatherDataAsync();
    Task<IEnumerable<WeatherDataDto>> GetWeatherTrendForCityAsync(string city, int hours);
}
