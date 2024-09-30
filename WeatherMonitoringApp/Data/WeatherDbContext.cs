using Microsoft.EntityFrameworkCore;
using WeatherMonitoringApp.Models;

namespace WeatherMonitoringApp.Data;

public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options)
    {
    }
    public DbSet<WeatherData> WeatherData { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Country> Countries { get; set; }
}