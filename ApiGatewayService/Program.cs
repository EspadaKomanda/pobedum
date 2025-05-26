using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Exceptions;

namespace ApiGatewayService;

public class Program
{
    public static void Main(string[] args)
    {
        ConfigureLogging();
        var builder = WebApplication.CreateBuilder(args);

        #region Swagger
        
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Auth service API", 
                Version = "v1",
                Description = "API микросервиса аутентификации",
                TermsOfService = new Uri("http://localhost:8080/terms"),
                Contact = new OpenApiContact
                {
                    Name = "API поддержки",
                    Url = new Uri("http://localhost:8080/support")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("http://opensource.org/licenses/MIT")
                },
                
            });
            
            
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        
        
        #endregion
        builder.Services.AddControllers();
        
        builder.Services.AddOpenApi();
        builder.Services.AddSerilog();
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization(); 

        app.MapControllers();

        app.Run();
    }

    static void ConfigureLogging(){
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json",optional:false,reloadOnChange:true).Build();
        Console.WriteLine(environment);
        Console.WriteLine(configuration);
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .WriteTo.Debug()
            .WriteTo.Console()
            .Enrich.WithProperty("Environment",environment)
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }
}