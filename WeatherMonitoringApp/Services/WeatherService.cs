using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WeatherMonitoringApp.Configuration;
using WeatherMonitoringApp.Exceptions;
using WeatherMonitoringApp.Models;
using WeatherMonitoringApp.Repositories;

namespace WeatherMonitoringApp.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly WeatherApiSettings _apiSettings;
    private readonly ILogger<WeatherService> _logger;
    private readonly IWeatherRepository _weatherRepository;

    private const string RapidApiKeyHeader = "X-RapidAPI-Key";
    private const string RapidApiHostHeader = "X-RapidAPI-Host";

    public WeatherService(IHttpClientFactory httpClientFactory, IOptions<EnvironmentSettings> settings, ILogger<WeatherService> logger, IWeatherRepository weatherRepository)
    {
        _httpClient = httpClientFactory.CreateClient();
        _apiSettings = settings.Value.WeatherApi;
        _logger = logger;
        _weatherRepository = weatherRepository;
    }

    public async Task SaveWeatherDataAsync(string city)
    {
        try
        {
            _logger.LogInformation($"Fetching weather data for {city}.");

            var request = PrepareRequest(city);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to fetch weather data for {city}. Status code: {response.StatusCode}");
                throw new WeatherServiceException($"Failed to fetch weather data for {city}.", (int)response.StatusCode);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var weatherResponse = JsonConvert.DeserializeObject<WeatherApiResponse>(responseContent);

            if (weatherResponse?.Location is null || weatherResponse.Current is null)
            {
                _logger.LogError($"Invalid data structure from Weather API for {city}.");
                throw new WeatherServiceException("Invalid data structure from Weather API.", 500);
            }
            await _weatherRepository.SaveWeatherDataAsync(city, weatherResponse);
        }
        catch (JsonSerializationException jsonEx)
        {
            _logger.LogError(jsonEx, $"Deserialization error when fetching weather data for {city}.");
            throw new WeatherServiceException("Failed to deserialize weather data.", 500);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, $"HTTP request error when fetching weather data for {city}.");
            throw new WeatherServiceException("Failed to send request to Weather API.", 500);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An unexpected error occurred when fetching or saving weather data for {city}.");
            throw;
        }
    }

    private HttpRequestMessage PrepareRequest(string city)
    {
        return new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{_apiSettings.Uri}?q={city}"),
            Headers =
            {
                { RapidApiKeyHeader, _apiSettings.ApiKey },
                { RapidApiHostHeader, _apiSettings.Host }
            }
        };
    }

    public async Task<IEnumerable<WeatherDataDto>> GetWeatherTrendForCityAsync(string city, int hours)
    {
        var thresholdTime = DateTime.UtcNow.AddHours(-hours);
        return await _weatherRepository.GetWeatherDataForCityFromLastHoursAsync(city, thresholdTime);
    }
    public async Task<IEnumerable<WeatherDataDto>> GetWeatherDataAsync()
    {
        return await _weatherRepository.GetWeatherDataAsync();
    }

}
