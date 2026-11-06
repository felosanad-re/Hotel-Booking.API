using Hotel_booking.Core.GenericRepository;
using Hotel_booking.Core.Options;
using Hotel_booking.Core.Services.Contract;
using Hotel_booking.Core.Services.Contract.TokenServices;
using Hotel_booking.Core.UnitOfWorks;
using Hotel_Booking.Repositories.GenericRepositories;
using Hotel_Booking.Repositories.UnitOfWorks;
using Hotel_Booking.Services;
using Hotel_Booking.Services.TokenServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Hotel_Booking.API.Extensions
{
    public static class ApplicationServices
    {

        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);


            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepo<>));

            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            // Add Dependence for CreateTokenService
            services.AddScoped<ICreateTokenService, CreateTokenService>();
            // Add Dependence for Initialization Database
            services.AddScoped<IDbInitialize, DbInitialize>();
            
            // Seeding Admin Options
            services.Configure<SeedAdminOptions>(configuration.GetSection(SeedAdminOptions.SectionName));

            // JWT Options
            var jwtSection = configuration.GetSection(JwtOptions.SectionName);
            services.Configure<JwtOptions>(jwtSection);

            var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(jwtOptions.ClockSkewMinutes),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
                };
            });

            return services;
        }

    }
}
