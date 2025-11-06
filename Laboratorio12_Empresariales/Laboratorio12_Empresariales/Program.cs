using Hangfire;
using Hangfire.MemoryStorage;
using Laboratorio12_Empresariales.Services;
using Microsoft.OpenApi.Models; // Asegúrate de tener este using para Swagger

var builder = WebApplication.CreateBuilder(args);

// Configurar Hangfire con MemoryStorage (sin base de datos)
builder.Services.AddHangfire(config => config
    .UseMemoryStorage()
    .UseFilter(new AutomaticRetryAttribute { Attempts = 3 })); // Deshabilitar reintentos ;
builder.Services.AddHangfireServer();

// Registrar servicio para inyección (NotificationService)
builder.Services.AddTransient<NotificationService>();

// Registrar servicio para la tarea personalizada
builder.Services.AddTransient<CustomTaskService>();

// Configurar Swagger para la documentación de la API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Laboratorio12 Empresariales API",
        Version = "v1",
        Description = "API para probar Hangfire con jobs fire-and-forget, delayed y recurrentes."
    });
});

var app = builder.Build();

// Middleware para Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Asegúrate de incluir este middleware
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Laboratorio12 Empresariales API v1");
        c.RoutePrefix = string.Empty; // Para acceder a Swagger UI en la raíz
    });
}

// Middleware para dashboard de Hangfire (colocado antes de otros middlewares)
app.UseHangfireDashboard("/hangfire");

app.UseHttpsRedirection();

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
    .WithName("GetWeatherForecast");

// Endpoint para encolar un fire-and-forget job
app.MapPost("/enqueue-notification", (IBackgroundJobClient backgroundJobs) =>
{
    // Opción 1: resolver vía DI (recomendado)
    backgroundJobs.Enqueue<NotificationService>(s => s.SendNotification("user1"));
    return Results.Ok("Enqueued fire-and-forget job");
});

// Endpoint para encolar un job diferido (schedule)
app.MapPost("/schedule-notification", (IBackgroundJobClient backgroundJobs) =>
{
    backgroundJobs.Schedule<NotificationService>(s => s.SendNotification("user2"), TimeSpan.FromMinutes(1));
    return Results.Ok("Scheduled job in 1 minute");
});

// Registrar job recurrente (se ejecutará según Cron.Daily)
RecurringJob.AddOrUpdate<NotificationService>("job-notificacion-diaria",
    s => s.SendNotification("user2"), Cron.Daily);

// Registrar job recurrente para limpieza de datos
RecurringJob.AddOrUpdate<CustomTaskService>("job-clean-old-data",
    s => s.CleanOldData(), Cron.Hourly);

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}