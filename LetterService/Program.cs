using LetterService.Extensions;
using Serilog;

namespace LetterService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddDatabase(builder.Configuration);
        builder.Services.AddRedisCaching(builder.Configuration);
        builder.Services.AddOpenApi();
        builder.Services.AddLogging();  
        builder.Services.AddSerilog();
        builder.Services.AddServices();
        builder.Services.AddCustomSwagger();
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}