using Tracker.Data;
using Microsoft.EntityFrameworkCore;
using Tracker.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddDbContext<TrackerContext>(options => options.UseSqlite("Data Source=Tracker.db"));
builder.Services.AddSqlite<TrackerContext>("Data Source=Tracker.db");

builder.Services.AddTransient<DbInitializer>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string Auth0Domain = builder.Configuration["Auth0Config:Domain"];
string Auth0ClientId = builder.Configuration["Auth0Config:AppClientId"];
string Auth0ClientSecret = builder.Configuration["Auth0Config:AppClientSecret"];
string Auth0Audience = builder.Configuration["Auth0Config:Audience"];

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/seed", (DbInitializer initializer) => 
{
    //initializer.Initialize();
});

app.MapGet("/races", async (TrackerContext db) =>
{
    return await db.Races.ToListAsync();
});

app.MapGet("/segments", async (TrackerContext db) =>
{
    return await db.Segments.ToListAsync();
});

app.MapGet("/checkpoints", async (TrackerContext db) =>
{
    return await db.Checkpoints.ToListAsync();
});

app.MapGet("/participants", async (TrackerContext db) =>
{
    return await db.Participants.ToListAsync();
});

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}