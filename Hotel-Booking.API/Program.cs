using Hotel_booking.Core.Models.Accounts;
using Hotel_Booking.API.Error_Handler;
using Hotel_Booking.API.Extensions;
using Hotel_Booking.Repositories.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Threading.Tasks;

namespace Hotel_Booking.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            try
            {
                // Serilog
                Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

                builder.Host.UseSerilog();

                #region Add services to the container.
                builder.Services.AddControllers();
                // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
                builder.Services.AddOpenApi();
                builder.Services.AddSwaggerGen();

                // Add Identity
                builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                }).AddEntityFrameworkStores<HotelDbContext>().AddDefaultTokenProviders();

                // Add DbContext
                builder.Services.AddDbContext<HotelDbContext>(optinos =>
                {
                    optinos.UseSqlServer(BuildConnectionString(builder.Configuration));
                });

                // Add Application Services
                builder.Services.AddApplicationServices(builder.Configuration);

                var allowedOrigin = builder.Configuration.GetSection("AllowCORS").Get<string[]>();
                var allowedOringin = builder.Configuration.GetSection("AllowCORS").Get<string[]>();
                builder.Services.AddCors(action =>
                {
                    action.AddPolicy("AngularPolicy", options =>
                    {
                        options.WithOrigins(allowedOrigin)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
                });
                #endregion

                var app = builder.Build();

                await app.InitializeDatabaseAsync();

                app.UseMiddleware<ExceptionMiddleware>();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.MapOpenApi();
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();

                app.UseCors("AngularPolicy");

                app.UseAuthentication();

                app.UseAuthorization();


                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                WriteStartupFailure(ex);
                throw;
            }
        }

        #region Build Connection String
        private static string BuildConnectionString(IConfiguration configuration)
        {
            var configurationConnectionString = configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(configurationConnectionString))
                throw new InvalidOperationException("Connection string 'Default' is missing.");

            var connectionStringBuilder = new SqlConnectionStringBuilder(configurationConnectionString);

            var passwordFromSecret = configuration["ConnectionStrings:DefaultPassword"];

            // Try Get Password From
            if (string.IsNullOrWhiteSpace(connectionStringBuilder.Password))
            {
                var passwordFromFallbackProvider = TryGetPasswordFromFallbackProvider(configuration);
                if(!string.IsNullOrWhiteSpace(passwordFromFallbackProvider))
                    connectionStringBuilder.Password = passwordFromFallbackProvider;
            }

            if (string.IsNullOrWhiteSpace(connectionStringBuilder.Password))
            {
                var passwordFromProductionFile = TryGetPasswordFromJsonFile(configuration, "appsettings.Production.Json");
                if(!string.IsNullOrWhiteSpace(passwordFromProductionFile))
                    connectionStringBuilder.Password = passwordFromProductionFile;
            }

            if (!string.IsNullOrWhiteSpace(passwordFromSecret))
                connectionStringBuilder.Password = passwordFromSecret;

            if (!connectionStringBuilder.IntegratedSecurity && string.IsNullOrWhiteSpace(connectionStringBuilder.Password))
            {
                throw new InvalidOperationException("Database password is missing. Set 'ConnectionStrings__DefaultPassword' or provide a full 'ConnectionStrings__Default' value outside git.");
            }

            return connectionStringBuilder.ConnectionString;
        }

        private static string? TryGetPasswordFromFallbackProvider(IConfiguration configuration)
        {
            if (configuration is not IConfigurationRoot configurationRoot)
            {
                return null;
            }

            foreach (var provider in configurationRoot.Providers)
            {
                if (!provider.TryGet("ConnectionStrings:Default", out var candidateConnectionString) ||
                    string.IsNullOrWhiteSpace(candidateConnectionString))
                {
                    continue;
                }

                try
                {
                    var candidateBuilder = new SqlConnectionStringBuilder(candidateConnectionString);
                    if (!string.IsNullOrWhiteSpace(candidateBuilder.Password))
                    {
                        return candidateBuilder.Password;
                    }
                }
                catch (ArgumentException)
                {
                    // Ignore malformed connection strings from optional providers and keep searching.
                }
            }

            return null;
        }

        private static string? TryGetPasswordFromJsonFile(IConfiguration configuration, string fileName)
        {
            var contentRoot = configuration.GetValue<string>(WebHostDefaults.ContentRootKey);
            if (string.IsNullOrWhiteSpace(contentRoot))
            {
                contentRoot = AppContext.BaseDirectory;
            }

            var fullPath = Path.Combine(contentRoot, fileName);
            if (!File.Exists(fullPath))
            {
                return null;
            }

            try
            {
                var directConfiguration = new ConfigurationBuilder()
                    .SetBasePath(contentRoot)
                    .AddJsonFile(fileName, optional: false, reloadOnChange: false)
                    .Build();

                var candidateConnectionString = directConfiguration.GetConnectionString("Default");
                if (string.IsNullOrWhiteSpace(candidateConnectionString))
                {
                    return null;
                }

                var candidateBuilder = new SqlConnectionStringBuilder(candidateConnectionString);
                return string.IsNullOrWhiteSpace(candidateBuilder.Password) ? null : candidateBuilder.Password;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Write Startup Errors in log file
        private static void WriteStartupFailure(Exception ex)
        {
            var message = $"""
            [{DateTime.UtcNow:0}] Application startup failed.
            {FlattenException(ex)}

            """;

            foreach(var logPath in GetStartupLogPaths())
            {
                try
                {
                    var directory = Path.GetDirectoryName(logPath);
                    if (!string.IsNullOrWhiteSpace(directory))
                        Directory.CreateDirectory(directory);

                    File.AppendAllText(logPath, message);
                }
                catch
                {
                    // Try the next writable location
                }
            }

            try
            {
                Console.Error.WriteLine(message);
            }
            catch
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        private static IEnumerable<string> GetStartupLogPaths()
        {
            var paths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "startup-error.log"),
                Path.Combine(AppContext.BaseDirectory, "logs", "startup-error.log"),
                Path.Combine(Path.GetTempPath(), "Hotel-Booking.API", "startup-error.log")
            };

            return paths.Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static string FlattenException(Exception ex)
        {
            var lines = new List<string>();
            var current = ex;
            var level = 0;

            while(current != null)
            {
                lines.Add($"level {level}: {current.GetType().FullName}");
                lines.Add($"Message: {current.Message}");
                lines.Add(current.StackTrace ?? "No stack trace available");
                lines.Add(string.Empty);
                current = current.InnerException;
                level++;
            }

            return string.Join(Environment.NewLine, lines);
        }
        #endregion
    }
}
