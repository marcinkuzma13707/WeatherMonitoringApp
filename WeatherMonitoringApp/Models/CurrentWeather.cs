using Newtonsoft.Json;

namespace WeatherMonitoringApp.Models;
public class CurrentWeather
{
    [JsonProperty("temp_c")]
    public double TempC { get; set; }
    [JsonProperty("wind_kph")]
    public double WindKph { get; set; }
    public int Cloud { get; set; }
    [JsonProperty("last_updated")]
    public string LastUpdated { get; set; }
}