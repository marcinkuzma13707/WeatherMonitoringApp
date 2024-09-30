using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using WeatherMonitoringApp.Configuration;
using WeatherMonitoringApp.Exceptions;
using WeatherMonitoringApp.Models;
using WeatherMonitoringApp.Repositories;
using WeatherMonitoringApp.Services;

namespace WeatherMonitoringAppTests;

public class WeatherServiceTests
{
    private HttpClient _httpClient;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IWeatherRepository> _weatherRepositoryMock;
    private readonly Mock<ILogger<WeatherService>> _loggerMock;
    private WeatherService _weatherService;
    private readonly WeatherApiSettings _apiSettings;

    public WeatherServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _weatherRepositoryMock = new Mock<IWeatherRepository>();
        _loggerMock = new Mock<ILogger<WeatherService>>();

        _apiSettings = new WeatherApiSettings
        {
            Uri = "https://api.weather.com",
            ApiKey = "fake-api-key",
            Host = "fake-host"
        };

        var options = Options.Create(new EnvironmentSettings { WeatherApi = _apiSettings });

        var mockHttpMessageHandler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        _httpClient = new HttpClient(mockHttpMessageHandler);
        _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(_httpClient);

        _weatherService = new WeatherService(_httpClientFactoryMock.Object, options, _loggerMock.Object, _weatherRepositoryMock.Object);
    }

    [Fact]
    public async Task SaveWeatherDataAsync_ShouldSaveDataCorrectly_WhenApiReturnsSuccess()
    {
        // Arrange
        var city = "Paris";
        var weatherApiResponse = new WeatherApiResponse
        {
            Location = new Location { Name = "Paris", Country = "France" },
            Current = new CurrentWeather { TempC = 15.5, WindKph = 20.1, Cloud = 80, LastUpdated = "2024-09-25 12:00" }
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(weatherApiResponse))
        };

        var mockHttpMessageHandler = new MockHttpMessageHandler(responseMessage);
        _httpClient = new HttpClient(mockHttpMessageHandler);
        _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(_httpClient);
        var options = Options.Create(new EnvironmentSettings { WeatherApi = _apiSettings });
        _weatherService = new WeatherService(_httpClientFactoryMock.Object, options, _loggerMock.Object, _weatherRepositoryMock.Object);

        // Act
        await _weatherService.SaveWeatherDataAsync(city);

        // Assert
        _weatherRepositoryMock.Verify(x => x.SaveWeatherDataAsync(city, It.IsAny<WeatherApiResponse>()), Times.Once);
    }

    [Fact]
    public async Task SaveWeatherDataAsync_ShouldThrowWeatherServiceException_WhenApiReturnsError()
    {
        // Arrange
        var city = "Paris";

        var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var mockHttpMessageHandler = new MockHttpMessageHandler(responseMessage);
        _httpClient = new HttpClient(mockHttpMessageHandler);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<WeatherServiceException>(() => _weatherService.SaveWeatherDataAsync(city));
        Assert.Equal(500, exception.StatusCode);
    }

    [Fact]
    public async Task SaveWeatherDataAsync_ShouldThrowWeatherServiceException_WhenJsonDeserializationFails()
    {
        // Arrange
        var city = "Paris";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json")
        };

        var mockHttpMessageHandler = new MockHttpMessageHandler(responseMessage);
        _httpClient = new HttpClient(mockHttpMessageHandler);

        // Act & Assert
        await Assert.ThrowsAsync<WeatherServiceException>(() => _weatherService.SaveWeatherDataAsync(city));
    }

    [Fact]
    public async Task SaveWeatherDataAsync_ShouldThrowWeatherServiceException_WhenApiReturnsInvalidDataStructure()
    {
        // Arrange
        var city = "Paris";
        var invalidResponse = new WeatherApiResponse
        {
            Location = null,
            Current = null
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(invalidResponse))
        };

        var mockHttpMessageHandler = new MockHttpMessageHandler(responseMessage);
        _httpClient = new HttpClient(mockHttpMessageHandler);

        // Act & Assert
        await Assert.ThrowsAsync<WeatherServiceException>(() => _weatherService.SaveWeatherDataAsync(city));
    }

    [Fact]
    public async Task GetWeatherTrendForCityAsync_ShouldReturnTrendData_WhenCalled()
    {
        // Arrange
        var city = "Paris";
        var weatherData = new List<WeatherDataDto>
        {
            new() { City = "Paris", Temperature = 15.5, WindSpeed = 20.1, Clouds = 80, LastUpdate = DateTime.UtcNow }
        };

        _weatherRepositoryMock.Setup(repo => repo.GetWeatherDataForCityFromLastHoursAsync(city, It.IsAny<DateTime>()))
            .ReturnsAsync(weatherData);

        // Act
        var result = await _weatherService.GetWeatherTrendForCityAsync(city, 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(weatherData, result);
        _weatherRepositoryMock.Verify(repo => repo.GetWeatherDataForCityFromLastHoursAsync(city, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task GetWeatherDataAsync_ShouldReturnWeatherData_WhenCalled()
    {
        // Arrange
        var weatherData = new List<WeatherDataDto>
        {
            new WeatherDataDto { City = "Paris", Temperature = 15.5, WindSpeed = 20.1, Clouds = 80, LastUpdate = DateTime.UtcNow }
        };

        _weatherRepositoryMock.Setup(repo => repo.GetWeatherDataAsync()).ReturnsAsync(weatherData);

        // Act
        var result = await _weatherService.GetWeatherDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(weatherData, result);
        _weatherRepositoryMock.Verify(repo => repo.GetWeatherDataAsync(), Times.Once);
    }
}