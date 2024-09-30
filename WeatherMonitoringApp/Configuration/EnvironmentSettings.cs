namespace WeatherMonitoringApp.Configuration;

public class WeatherApiSettings
{
    public string Uri { get; set; }
    public string Host { get; set; }
    public string ApiKey { get; set; }
}

public class EnvironmentSettings
{
    public WeatherApiSettings WeatherApi { get; set; }
    public List<string> Cities { get; set; }
}
