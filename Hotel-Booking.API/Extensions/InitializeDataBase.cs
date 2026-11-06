using Hotel_booking.Core.Services.Contract;
using Hotel_Booking.Repositories.Data;
using Hotel_Booking.Repositories.Data.DataSeeding;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Booking.API.Extensions
{
    public static class InitializeDataBase
    {

        public static async Task InitializeDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var services = scope.ServiceProvider;

            var _context = services.GetRequiredService<HotelDbContext>();
            var _dbInitialization = services.GetRequiredService<IDbInitialize>();
            var logger = services.GetRequiredService<ILoggerFactory>();

            try
            {
                await _context.Database.MigrateAsync();
                await _dbInitialization.CreateInitializationAsync();
                await HotelDbContextSeeder.SeederAsync(_context);
            }
            catch (Exception ex)
            {
                var _logger = logger.CreateLogger<Program>();
                _logger.LogError(ex, "Error in database");
            }
        }
    }
}
