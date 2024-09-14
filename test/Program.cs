using Microsoft.Data.SqlClient;
using YourNamespace.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<Test>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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
