using Microsoft.EntityFrameworkCore;
using WeatherMonitoringApp.Data;
using WeatherMonitoringApp.Models;
using WeatherMonitoringApp.Repositories;

namespace WeatherMonitoringAppTests;

public class WeatherRepositoryTests
{
    private WeatherDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<WeatherDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new WeatherDbContext(options);
    }

    [Fact]
    public async Task AddWeatherDataAsync_ShouldAddWeatherDataToDatabase()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext(nameof(AddWeatherDataAsync_ShouldAddWeatherDataToDatabase));
        var repository = new WeatherRepository(dbContext);

        var weatherData = new WeatherData
        {
            Temperature = 15.5,
            WindSpeed = 20.1,
            Clouds = 80,
            LastUpdate = DateTime.UtcNow,
            City = new City { Id = Guid.NewGuid(), Name = "Paris", Country = new Country { Id = Guid.NewGuid(), Name = "France" } }
        };

        // Act
        await repository.AddWeatherDataAsync(weatherData);

        // Assert
        var savedData = await dbContext.WeatherData.FirstOrDefaultAsync();
        Assert.NotNull(savedData);
        Assert.Equal(15.5, savedData.Temperature);
        Assert.Equal(20.1, savedData.WindSpeed);
        Assert.Equal(80, savedData.Clouds);
    }

    [Fact]
    public async Task GetWeatherDataForCityFromLastHoursAsync_ShouldReturnWeatherDataForCityWithinLastHours()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext(nameof(GetWeatherDataForCityFromLastHoursAsync_ShouldReturnWeatherDataForCityWithinLastHours));
        var repository = new WeatherRepository(dbContext);

        var city = new City { Id = Guid.NewGuid(), Name = "Paris", Country = new Country { Id = Guid.NewGuid(), Name = "France" } };
        var weatherData = new List<WeatherData>
        {
            new() { City = city, Temperature = 15.5, WindSpeed = 20.1, Clouds = 80, LastUpdate = DateTime.UtcNow.AddHours(-1) },
            new() { City = city, Temperature = 17.5, WindSpeed = 25.1, Clouds = 70, LastUpdate = DateTime.UtcNow.AddHours(-2) },
            new() { City = city, Temperature = 13.5, WindSpeed = 10.1, Clouds = 90, LastUpdate = DateTime.UtcNow.AddHours(-5) }
        };
        dbContext.WeatherData.AddRange(weatherData);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetWeatherDataForCityFromLastHoursAsync("Paris", DateTime.UtcNow.AddHours(-3));

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, r => Assert.Equal("Paris", r.City));
        Assert.All(result, r => Assert.True(r.LastUpdate >= DateTime.UtcNow.AddHours(-3)));
    }

    [Fact]
    public async Task GetWeatherDataAsync_ShouldReturnAllWeatherData()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext(nameof(GetWeatherDataAsync_ShouldReturnAllWeatherData));
        var repository = new WeatherRepository(dbContext);

        var city1 = new City { Id = Guid.NewGuid(), Name = "Paris", Country = new Country { Id = Guid.NewGuid(), Name = "France" } };
        var city2 = new City { Id = Guid.NewGuid(), Name = "Berlin", Country = new Country { Id = Guid.NewGuid(), Name = "Germany" } };

        var weatherData = new List<WeatherData>
        {
            new() { City = city1, Temperature = 15.5, WindSpeed = 20.1, Clouds = 80, LastUpdate = DateTime.UtcNow },
            new() { City = city2, Temperature = 10.5, WindSpeed = 15.1, Clouds = 60, LastUpdate = DateTime.UtcNow }
        };
        dbContext.WeatherData.AddRange(weatherData);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetWeatherDataAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.City == "Paris" && r.Country == "France");
        Assert.Contains(result, r => r.City == "Berlin" && r.Country == "Germany");
    }

    [Fact]
    public async Task SaveWeatherDataAsync_ShouldCreateNewCountryAndCity_WhenTheyDoNotExist()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext(nameof(SaveWeatherDataAsync_ShouldCreateNewCountryAndCity_WhenTheyDoNotExist));
        var repository = new WeatherRepository(dbContext);

        var weatherResponse = new WeatherApiResponse
        {
            Location = new Location { Name = "Tokyo", Country = "Japan" },
            Current = new CurrentWeather { TempC = 25.0, WindKph = 30.0, Cloud = 50, LastUpdated = "2024-09-25 12:00" }
        };

        // Act
        await repository.SaveWeatherDataAsync("Tokyo", weatherResponse);

        // Assert
        var savedCountry = await dbContext.Countries.FirstOrDefaultAsync(c => c.Name == "Japan");
        var savedCity = await dbContext.Cities.FirstOrDefaultAsync(c => c.Name == "Tokyo" && c.CountryId == savedCountry.Id);
        var savedWeatherData = await dbContext.WeatherData.FirstOrDefaultAsync(w => w.CityId == savedCity.Id);

        Assert.NotNull(savedCountry);
        Assert.NotNull(savedCity);
        Assert.NotNull(savedWeatherData);
        Assert.Equal(25.0, savedWeatherData.Temperature);
        Assert.Equal(30.0, savedWeatherData.WindSpeed);
        Assert.Equal(50, savedWeatherData.Clouds);
    }
}
