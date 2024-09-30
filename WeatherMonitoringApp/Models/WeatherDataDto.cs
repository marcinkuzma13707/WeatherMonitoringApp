namespace WeatherMonitoringApp.Models;

public class WeatherDataDto
{
    public string City { get; set; }
    public string Country { get; set; }
    public double Temperature { get; set; }
    public double WindSpeed { get; set; }
    public int Clouds { get; set; }
    public DateTime LastUpdate { get; set; }
}
