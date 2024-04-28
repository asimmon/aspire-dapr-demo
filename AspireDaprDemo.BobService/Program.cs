using AspireDaprDemo.ServiceDefaults;
using Dapr.Client;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapSwaggerEndpoints();

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", async (DaprClient client) =>
    {
        // Simulating a flaky API in order to demonstrate Dapr's resilience features
        if (Random.Shared.NextDouble() > 0.4)
        {
            throw new InvalidOperationException("Something wrong happened");
        }

        // Service "carol" is listening to this topic
        await client.PublishEventAsync("pubsub", "weather", new WeatherForecastMessage("Weather forecast requested!"));

        return Enumerable.Range(1, 5)
            .Select(index => new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapDefaultEndpoints();

app.Run();
