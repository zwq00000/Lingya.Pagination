using Microsoft.EntityFrameworkCore;

namespace Samples {
    public class AppDbContext : DbContext {
        public AppDbContext (DbContextOptions<AppDbContext> options) : base (options) {

        }

        public DbSet<WeatherForecast> WeatherForecasts { get; set; }

    }
}