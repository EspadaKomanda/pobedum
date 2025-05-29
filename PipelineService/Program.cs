using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PipelineService.Autumn.Extensions;
using PipelineService.Communicators;
using PipelineService.Database;
using PipelineService.Database.Repositories;
using PipelineService.Services.Pipeline;
using PipelineService.Services.Utils;
using PipelineService.Utils;
using Serilog;
using Serilog.Exceptions;


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
        
        #region Database

        builder.Services.AddDbContext<ApplicationContext>(x =>
        {
            var dbSettings =  builder.Configuration.GetSection("DatabaseSettings");
            var hostname = dbSettings["Hostname"] ?? "localhost";
            var port = dbSettings["Port"] ?? "5432";
            var name = dbSettings["Name"] ?? "postgres";
            var username = dbSettings["Username"] ?? "postgres";
            var password = dbSettings["Password"] ?? "postgres";
            x.UseNpgsql($"Server={hostname}:{port};Database={name};Uid={username};Pwd={password};");
        });
        builder.Services.AddSingleton(typeof(GenericRepository<>));
        builder.Services.AddScoped<UnitOfWork>(sp => new UnitOfWork(sp.GetRequiredService<ApplicationContext>()));

        #endregion

        
        builder.Services.AddControllers();
        builder.Services.AddHttpClient();
        
        builder.Services.AddSingleton<MicroservicesHttpClient>();
        builder.Services.AddOpenApi();
      
        builder.Services.AddSerilog();
        builder.Services.AddSingleton<Pipeline>();
        builder.Services.AddSingleton<VideosCommunicator>();
        builder.Services.AddScoped<IPipelineService, PipelineService.Services.Pipeline.PipelineService>();
        builder.Services.AddKafka(options =>
        {
            options.ProducerConfig = new Confluent.Kafka.ProducerConfig
            {
                BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092"
            };
            options.ConsumerConfig = new Confluent.Kafka.ConsumerConfig
            {
                BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = builder.Configuration["Kafka:GroupId"] ?? "pipeline-service-group",
                AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest
            };
            options.AdminClientConfig = new Confluent.Kafka.AdminClientConfig
            {
                BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092"
            };
        });
        
        
        
        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();  

        app.UseAuthorization();
        app.UseKafka(Assembly.GetExecutingAssembly());
        
        app.UseSwagger();
        app.UseSwaggerUI();
        
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