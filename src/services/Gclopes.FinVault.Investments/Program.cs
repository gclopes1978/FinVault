using Gclopes.FinVault.Investments.Data;
using Gclopes.FinVault.Investments.Endpoints;
using Gclopes.FinVault.Investments.Services.Implementations;
using Gclopes.FinVault.Investments.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddControllers();
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
builder.Services.AddDbContext<FinVaultInvestmentsDb>(options =>
    options.UseSqlServer(connectionString));

// Register services
builder.Services.AddScoped<IInvestmentsService, InvestmentsService>();
builder.Services.AddScoped<IXtbCsvImporter, XtbCsvImporter>();
builder.Services.AddScoped<ITrading212CsvImporter, Trading212CsvImporter>();


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
    // Enable the underlying application engine middleware payload processor to serve generated Swagger JSON documents
    app.UseSwagger();
    // Enable core UI dashboard presentation assets to easily explore interactive live gateway API documentation
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapInvestmentsEndpoints();

app.Run();
