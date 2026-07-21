using Gclopes.FinVault.MarketPrices.Configurations;
using Gclopes.FinVault.MarketPrices.Interfaces;
using Gclopes.FinVault.MarketPrices.Providers;
using Gclopes.FinVault.MarketPrices.Resolver;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ---------------------------------------------------------
// Configuration (Yahoo Finance)
builder.Services.Configure<YahooFinanceOptions>(
    builder.Configuration.GetSection("YahooFinance"));

// ---------------------------------------------------------
// Dependency Injection
// ---------------------------------------------------------
builder.Services.AddHttpClient<IMarketPriceProvider, YahooFinancePriceProvider>("Yahoo", client =>
{
    client.BaseAddress = new Uri("https://query1.finance.yahoo.com/v8/finance/chart/");
    client.DefaultRequestHeaders.Add("User-Agent", "FinVault-App/1.0");
});


builder.Services.Configure<CoingeckoOptions>(
    builder.Configuration.GetSection("Coingecko"));

builder.Services.AddHttpClient<IMarketPriceProvider, CoingeckoPriceProvider>("Coingecko", client =>
{
    client.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
    client.DefaultRequestHeaders.Add("User-Agent", "FinVault-App/1.0");
});

// Resolver
builder.Services.AddSingleton<IMarketPriceResolver>(sp =>
{
    var yahoo = sp.GetRequiredService<IHttpClientFactory>().CreateClient("Yahoo");
    var crypto = sp.GetRequiredService<IHttpClientFactory>().CreateClient("Coingecko");

    return new MarketPriceResolver(
        new YahooFinancePriceProvider(yahoo),
        new CoingeckoPriceProvider(crypto)
    );
});

var app = builder.Build();

// ---------------------------------------------------------
// Run
// ---------------------------------------------------------
app.Run();