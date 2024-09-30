using WeatherMonitoringApp.Models;
namespace WeatherMonitoringApp.Repositories;

public interface IWeatherRepository
{
    Task SaveWeatherDataAsync(string cityName, WeatherApiResponse weatherResponse);
    Task AddWeatherDataAsync(WeatherData weatherData);
    Task<IEnumerable<WeatherDataDto>> GetWeatherDataAsync();
    Task<IEnumerable<WeatherDataDto>> GetWeatherDataForCityFromLastHoursAsync(string city, DateTime thresholdTime);
}