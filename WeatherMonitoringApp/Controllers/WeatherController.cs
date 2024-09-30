using Microsoft.AspNetCore.Mvc;
using WeatherMonitoringApp.Services;

namespace WeatherMonitoringApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet("weather-trend")]
    public async Task<IActionResult> GetWeatherTrend([FromQuery] string city, [FromQuery] int hours = 2)
    {
        var result = await _weatherService.GetWeatherTrendForCityAsync(city, hours);
        return Ok(result);
    }

    [HttpGet("weather-data")]
    public async Task<IActionResult> GetWeatherData()
    {
        var result = await _weatherService.GetWeatherDataAsync();
        return Ok(result);
    }
}

