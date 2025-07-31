using ExchangeComparator.Domain.Models;
using ExchangeComparator.Domain.Interfaces;
using ExchangeComparator.Application.Services;
using ExchangeComparator.Infrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ExchangeRateService>();
builder.Services.AddLogging();

builder.Services.AddHttpClient<FirstApiProvider>(client =>
{
    client.BaseAddress = new Uri("https://api1.example.com");
});

builder.Services.AddHttpClient<SecondApiProvider>(client =>
{
    client.BaseAddress = new Uri("https://api2.example.com");
});

builder.Services.AddHttpClient<ThirdApiProvider>(client =>
{
    client.BaseAddress = new Uri("https://api3.example.com");
});


builder.Services.AddScoped<IExchangeRateProvider, FirstApiProvider>();
builder.Services.AddScoped<IExchangeRateProvider, SecondApiProvider>();
builder.Services.AddScoped<IExchangeRateProvider, ThirdApiProvider>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Exchange Rate Comparator API");

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.MapPost("/api/v1/best-rate", async (
    ExchangeRateRequest request,
    ExchangeRateService service) =>
{
    var bestRate = await service.GetBestRateAsync(request);
    return Results.Ok(bestRate);
});

app.Run();
