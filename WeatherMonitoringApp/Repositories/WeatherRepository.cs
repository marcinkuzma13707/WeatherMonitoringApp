using Microsoft.EntityFrameworkCore;
using WeatherMonitoringApp.Data;
using WeatherMonitoringApp.Models;

namespace WeatherMonitoringApp.Repositories;

public class WeatherRepository : IWeatherRepository
{
    private readonly WeatherDbContext _context;

    public WeatherRepository(WeatherDbContext context)
    {
        _context = context;
    }

    public async Task AddWeatherDataAsync(WeatherData weatherData)
    {
        _context.WeatherData.Add(weatherData);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<WeatherDataDto>> GetWeatherDataForCityFromLastHoursAsync(string city, DateTime thresholdTime)
    {
        return await _context.WeatherData
            .Include(w => w.City)
            .ThenInclude(c => c.Country)
            .Where(w => w.City.Name == city && w.LastUpdate >= thresholdTime)
            .OrderBy(w => w.LastUpdate)
            .Select(w => new WeatherDataDto
            {
                City = w.City.Name,
                Country = w.City.Country.Name,
                Temperature = w.Temperature,
                WindSpeed = w.WindSpeed,
                Clouds = w.Clouds,
                LastUpdate = w.LastUpdate
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<WeatherDataDto>> GetWeatherDataAsync()
    {
        return await _context.WeatherData
            .Include(w => w.City)
            .ThenInclude(c => c.Country)
            .Select(w => new WeatherDataDto
            {
                City = w.City.Name,
                Country = w.City.Country.Name,
                Temperature = w.Temperature,
                WindSpeed = w.WindSpeed,
                Clouds = w.Clouds,
                LastUpdate = w.LastUpdate
            })
            .ToListAsync();
    }

    public async Task SaveWeatherDataAsync(string cityName, WeatherApiResponse weatherResponse)
    {
        var country = await _context.Countries.FirstOrDefaultAsync(c => c.Name == weatherResponse.Location.Country);
        if (country == null)
        {
            country = new Country { Id = Guid.NewGuid(), Name = weatherResponse.Location.Country };
            _context.Countries.Add(country);
        }

        var city = await _context.Cities.FirstOrDefaultAsync(c => c.Name == weatherResponse.Location.Name && c.CountryId == country.Id);
        if (city == null)
        {
            city = new City { Id = Guid.NewGuid(), Name = weatherResponse.Location.Name, CountryId = country.Id };
            _context.Cities.Add(city);
        }

        var weatherData = new WeatherData
        {
            CityId = city.Id,
            Temperature = weatherResponse.Current.TempC,
            WindSpeed = weatherResponse.Current.WindKph,
            Clouds = weatherResponse.Current.Cloud,
            LastUpdate = DateTime.UtcNow
        };

        await _context.WeatherData.AddAsync(weatherData);
        await _context.SaveChangesAsync();
    }
}
