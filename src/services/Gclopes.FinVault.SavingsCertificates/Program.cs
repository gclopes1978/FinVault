using Gclopes.FinVault.SavingsCertificates.Data;
using Gclopes.FinVault.SavingsCertificates.Endpoints;
using Gclopes.FinVault.SavingsCertificates.Services.Implementations;
using Gclopes.FinVault.SavingsCertificates.Services.Interfaces;
using Gclopes.FinVault.SavingsCertificates.Services.Strategies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

// Register DbContexts
builder.Services.AddDbContext<SavingsBondDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// Register Savings Certificates services
builder.Services.AddScoped<ISeriesStrategy, SeriesEStrategy>();
builder.Services.AddScoped<ISeriesStrategy, SeriesFStrategy>();
builder.Services.AddScoped<IInterestCalculationService, InterestCalculationService>();

builder.Services.AddLogging();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapOpenApi();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapSavingsBondsEndpoints();
app.MapSeriesRateHistoryEndpoints();

// Ensure databases are created on startup
using (var scope = app.Services.CreateScope())
{
    var provider = scope.ServiceProvider;

    var savingsDb = provider.GetRequiredService<SavingsBondDbContext>();
    savingsDb.Database.EnsureCreated();
}

app.Run();