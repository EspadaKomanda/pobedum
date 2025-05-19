using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using LetterService.Database;
using LetterService.Database.Repositories;
using LetterService.Logs;
using LetterService.Services.Favourites;
using LetterService.Services.Letters;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

namespace LetterService.Extensions;



public static class CachingExtensions
{
    public static IServiceCollection AddRedisCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:ConnectionString"] ?? "localhost:6379";
            options.InstanceName = configuration["Redis:InstanceName"] ?? "default";
        });
        return services;
    }
}

public static class LoggingExtensions
{
    public static IHostBuilder UseCustomSerilog(this IHostBuilder hostBuilder)
    {
        LoggingConfigurator.ConfigureLogging();
        return hostBuilder.UseSerilog();
    }
}

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationContext>(x =>
        {
            var dbSettings = configuration.GetSection("DatabaseSettings");
            var hostname = dbSettings["Hostname"] ?? "localhost";
            var port = dbSettings["Port"] ?? "5432";
            var name = dbSettings["Name"] ?? "postgres";
            var username = dbSettings["Username"] ?? "postgres";
            var password = dbSettings["Password"] ?? "postgres";
            x.UseNpgsql($"Server={hostname}:{port};Database={name};Uid={username};Pwd={password};");
        });
        services.AddScoped(typeof(GenericRepository<>));
        services.AddScoped<UnitOfWork>(sp => new UnitOfWork(sp.GetRequiredService<ApplicationContext>()));
        return services;
    }
}

public static class SwaggerExtensions
{
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        string projectName = Assembly.GetExecutingAssembly().GetName().Name;
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = projectName, Version = "v1" });

            // Set the comments path for the Swagger JSON and UI
            var xmlFile = $"{projectName}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
        return services;
    }
}

public static class ServicesExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ILetterService, Services.Letters.LetterService>();
        services.AddScoped<IFavouritesService,FavouritesService>();
        return services;
    }
}

