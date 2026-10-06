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

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
     "Connection string 'DefaultConnection' is not configured (compose: SQLSERVER_CONNECTION_INVESTMENTS).");
}

builder.Services.AddDbContext<FinVaultInvestmentsDb>(options =>
options.UseSqlServer(connectionString, sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(30),
        errorNumbersToAdd: null);
}));


// Register services
builder.Services.AddScoped<IInvestmentsService, InvestmentsService>();
builder.Services.AddScoped<ITrading212CsvImporter, Trading212CsvImporter>();
builder.Services.AddScoped<IXtbExcelImporter, XtbExcelImporter>();
builder.Services.AddAuthorization();

var jwtSecret = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("JwtSettings:Secret is not configured.");

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
       options.SwaggerEndpoint("/openapi/v1.json", "Investments API")); // UI at /swagger

}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapInvestmentsEndpoints();


// Aplicar migrations apenas à BD existente
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<FinVaultInvestmentsDb>();

//    if (dbContext.Database.CanConnect())
//    {
//        dbContext.Database.Migrate();
//    }
//}

app.Run();
