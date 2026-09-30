using Gclopes.FinVault.MarketPrices.Configurations;
using Gclopes.FinVault.MarketPrices.Endpoints;
using Gclopes.FinVault.MarketPrices.Interfaces;
using Gclopes.FinVault.MarketPrices.Middlewares;
using Gclopes.FinVault.MarketPrices.Providers;
using Gclopes.FinVault.MarketPrices.Resolver;
using Gclopes.FinVault.MarketPrices.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");
// ---------------------------------------------------------
// API Documentation & Tools
// ---------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// ---------------------------------------------------------
// CORS Configuration
// ---------------------------------------------------------
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
// JWT Authentication
// ---------------------------------------------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Token validation configuration
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"] ??
                throw new InvalidOperationException("JwtSettings:Secret is missing!")))
        };
    });

builder.Services.AddAuthorization();

// ---------------------------------------------------------
// External HTTP Client Settings (from appsettings.json)
// ---------------------------------------------------------
var yahooSettings = builder.Configuration.GetSection("HttpClients:YahooFinance").Get<HttpClientSettings>()
    ?? throw new InvalidOperationException("Missing or invalid configuration for HttpClients:YahooFinance");

var yahooFinanceSearchSettings = builder.Configuration.GetSection("HttpClients:YahooFinanceSearch").Get<HttpClientSettings>()
    ?? throw new InvalidOperationException("Missing or invalid configuration for HttpClients:YahooFinanceSearch");

var coingeckoSettings = builder.Configuration.GetSection("HttpClients:Coingecko").Get<HttpClientSettings>()
    ?? throw new InvalidOperationException("Missing or invalid configuration for HttpClients:Coingecko");


// ---------------------------------------------------------
// Polly Resilience Configuration (Retry, Timeout, Circuit Breaker)
// ---------------------------------------------------------
var pollyOptions = new PollyOptions
{
    RetryCount = builder.Configuration.GetRequiredSection("Polly").GetValue<int>("RetryCount"),
    RetryBaseDelaySeconds = builder.Configuration.GetRequiredSection("Polly").GetValue<int>("RetryBaseDelaySeconds"),
    TimeoutSeconds = builder.Configuration.GetRequiredSection("Polly").GetValue<int>("TimeoutSeconds"),
    CircuitBreaker = new CircuitBreakerOptions
    {
        ExceptionsAllowedBeforeBreaking = builder.Configuration.GetRequiredSection("Polly:CircuitBreaker")
            .GetValue<int>("ExceptionsAllowedBeforeBreaking"),
        DurationOfBreakSeconds = builder.Configuration.GetRequiredSection("Polly:CircuitBreaker")
            .GetValue<int>("DurationOfBreakSeconds")
    }
};

// Retry policy with exponential backoff
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        pollyOptions.RetryCount,
        attempt => TimeSpan.FromSeconds(Math.Pow(pollyOptions.RetryBaseDelaySeconds, attempt))
    );

// Timeout policy
var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
    pollyOptions.TimeoutSeconds
);

// Circuit breaker policy
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        pollyOptions.CircuitBreaker.ExceptionsAllowedBeforeBreaking,
        TimeSpan.FromSeconds(pollyOptions.CircuitBreaker.DurationOfBreakSeconds)
    );

// ---------------------------------------------------------
// HTTP Clients (Yahoo + Coingecko) with Polly Policies
// ---------------------------------------------------------
builder.Services.AddHttpClient("Yahoo", client =>
{
    client.BaseAddress = new Uri(yahooSettings.BaseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", yahooSettings.UserAgent);
})
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(timeoutPolicy)
.AddPolicyHandler(circuitBreakerPolicy);

builder.Services.AddHttpClient("YahooFinanceSearch", client =>
{
    client.BaseAddress = new Uri(yahooFinanceSearchSettings.BaseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", yahooFinanceSearchSettings.UserAgent);
});


builder.Services.AddHttpClient("Coingecko", client =>
{
    client.BaseAddress = new Uri(coingeckoSettings.BaseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", coingeckoSettings.UserAgent);
})
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(timeoutPolicy)
.AddPolicyHandler(circuitBreakerPolicy);

// ---------------------------------------------------------
// Providers (Yahoo + Coingecko)
// ---------------------------------------------------------
builder.Services.AddSingleton(sp =>
{
    // Yahoo provider using resilient HttpClient
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("Yahoo");
    return new YahooFinancePriceProvider(client);
});

builder.Services.AddSingleton(sp =>
{
    // Coingecko provider using resilient HttpClient
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("Coingecko");
    return new CoingeckoPriceProvider(client);
});

// ---------------------------------------------------------
// Resolver (combines both providers)
// ---------------------------------------------------------
builder.Services.AddSingleton<IMarketPriceResolver>(sp =>
{
    var yahoo = sp.GetRequiredService<YahooFinancePriceProvider>();
    var crypto = sp.GetRequiredService<CoingeckoPriceProvider>();
    var factory = sp.GetRequiredService<IHttpClientFactory>();

    return new MarketPriceResolver(yahoo, crypto, factory);
});


// ---------------------------------------------------------
// Application Services
// ---------------------------------------------------------
builder.Services.AddScoped<IMarketPriceService, MarketPriceService>();

// ---------------------------------------------------------
// Health Checks
// ---------------------------------------------------------
builder.Services.AddHealthChecks()
   .AddCheck("self", () => HealthCheckResult.Healthy("Service is running"));

// ---------------------------------------------------------
// Logging
// ---------------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ---------------------------------------------------------
// Build Application
// ---------------------------------------------------------
var app = builder.Build();

// Global exception handler middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Health check endpoint
app.MapHealthChecks("/health");

// OpenAPI endpoint
app.MapOpenApi();

// Standard middleware pipeline
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// API endpoints
app.MapMarketPricesEndpoints();

// ---------------------------------------------------------
// Run Application
// ---------------------------------------------------------
app.Run();
