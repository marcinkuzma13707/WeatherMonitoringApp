using Microsoft.Extensions.Options;
using WeatherMonitoringApp.Configuration;
using WeatherMonitoringApp.Data;
using WeatherMonitoringApp.Exceptions;

namespace WeatherMonitoringApp.Services;

public class WeatherBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WeatherBackgroundService> _logger;
    private readonly List<string> _cities;

    public WeatherBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<EnvironmentSettings> settings,
        ILogger<WeatherBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _cities = settings.Value.Cities;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var weatherService = scope.ServiceProvider.GetRequiredService<IWeatherService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();

                foreach (var city in _cities)
                {
                    try
                    {
                        _logger.LogInformation($"Fetching weather data for {city}.");
                        await weatherService.SaveWeatherDataAsync(city);
                    }
                    catch (WeatherServiceException ex)
                    {
                        _logger.LogError(ex, $"Error fetching weather data for {city}: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"An unexpected error occurred for {city}: {ex.Message}");
                    }
                }

                try
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving weather data to database.");
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(601), stoppingToken);
        }
    }
}
