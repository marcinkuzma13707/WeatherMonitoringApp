namespace WeatherMonitoringApp.Models;

public class WeatherData
{
    public Guid Id { get; set; }
    public Guid CityId { get; set; }
    public City City { get; set; }
    public double Temperature { get; set; }
    public double WindSpeed { get; set; }
    public int Clouds { get; set; }
    public DateTime LastUpdate { get; set; }
}
