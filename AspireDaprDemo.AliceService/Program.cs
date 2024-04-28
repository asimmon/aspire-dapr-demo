using AspireDaprDemo.ServiceDefaults;
using Dapr.Client;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapSwaggerEndpoints();

app.UseCloudEvents();
app.MapSubscribeHandler();

app.MapGet("/weatherforecast", async (DaprClient client) =>
    {
        // Attempt to get the cached weather forecast from Dapr state store
        var cachedForecasts = await client.GetStateAsync<CachedWeatherForecast>("statestore", "cache");

        if (cachedForecasts is not null && cachedForecasts.CachedAt > DateTimeOffset.UtcNow.AddMinutes(-1))
        {
            return cachedForecasts.Forecasts;
        }

        // Otherwise, get a fresh weather forecast from the flaky service "bob" and cache it
        var forecasts = await client.InvokeMethodAsync<WeatherForecast[]>(HttpMethod.Get, "bob", "weatherforecast");

        await client.SaveStateAsync("statestore", "cache", new CachedWeatherForecast(forecasts, DateTimeOffset.UtcNow));

        return forecasts;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapDefaultEndpoints();

app.Run();
