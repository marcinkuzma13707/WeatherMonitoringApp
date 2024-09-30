namespace WeatherMonitoringApp.Models;

public class WeatherApiResponse
{
    public Location Location { get; set; }
    public CurrentWeather Current { get; set; }
}