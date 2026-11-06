using Hotel_booking.Core.Options;
using Hotel_booking.Core.Services.Contract;
using Hotel_Booking.Services;

namespace Hotel_Booking.API.Extensions
{
    public static class ApplicationServices
    {

        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {


            // Add Dependence for Initialization Database
            services.AddScoped<IDbInitialize, DbInitialize>();
            // Seeding Admin Options
            services.Configure<SeedAdminOptions>(configuration.GetSection(SeedAdminOptions.SectionName));

            return services;
        }

    }
}
