
using ApiGatewayService.Communicators;
using ApiGatewayService.Handlers;
using ApiGatewayService.Services;
using ApiGatewayService.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Exceptions;

        ConfigureLogging();
        var builder = WebApplication.CreateBuilder(args);

        #region Swagger
        
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API Gateway Service", 
                Version = "v1",
                Description = "API Gateway for microservices",
                TermsOfService = new Uri("http://localhost:8080/terms"),
                Contact = new OpenApiContact
                {
                    Name = "API Support",
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
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
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

        builder.Services.AddHttpClient();
        
        builder.Services.AddSingleton<MicroservicesHttpClient>();
        
        builder.Services.AddSingleton<AuthCommunicator>();
        builder.Services.AddSingleton<LettersCommunicator>();
        builder.Services.AddSingleton<VideosCommunicator>();
        builder.Services.AddSingleton<VideoGenerationCommunicator>();
        
        // Configure authentication
        builder.Services
            .AddAuthentication("Basic").AddScheme<MicroservicesAuthenticationOptions, MicroservicesAuthentificationHandler>("Basic",null);
            
        // Configure authorization policies
        
        
        builder.Services.AddOpenApi();
        builder.Services.AddSerilog();
        
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseSwagger();
        app.UseSwaggerUI();
        
        app.UseAuthentication();
        app.UseAuthorization(); 

        app.MapControllers();

        app.Run();
    

void ConfigureLogging(){
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