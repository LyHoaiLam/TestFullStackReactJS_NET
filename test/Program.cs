using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using YourNamespace.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DatabaseService and TextAnalysisService
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<Test>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Define WeatherForecast endpoint
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/firstmessages", async (DatabaseService dbService) =>
{
    var results = await dbService.GetFirstMessagesAsync();
    return Results.Ok(results);
})
.WithName("GetFirstMessages")
.WithOpenApi();

app.MapGet("/averageletters", (Test textService, string sentence) =>
{
    var averageLetters = textService.AverageLettersPerWord(sentence);
    return Results.Ok(new { AverageLettersPerWord = averageLetters });
})
.WithName("GetAverageLetters")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// DatabaseService class
public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnect");
    }

    public async Task<IEnumerable<dynamic>> GetFirstMessagesAsync()
    {
        var query = @"
            SELECT 
                AdID, 
                MIN(LogDate) AS FirstLogDate
            FROM 
                Messages
            GROUP BY 
                AdID;
        ";

        var results = new List<dynamic>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(query, connection))
            {
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    results.Add(new
                    {
                        AdID = reader["AdID"],
                        FirstLogDate = reader["FirstLogDate"]
                    });
                }
            }
        }

        return results;
    }
}
