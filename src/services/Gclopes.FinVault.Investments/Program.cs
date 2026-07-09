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
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Certifica-te que usas a chave definida no teu .env
// Substitui a linha do connectionString por esta:
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'SQLSERVER_CONNECTION_INVESTMENTS' not found.");
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
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapOpenApi();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapInvestmentsEndpoints();


// Garantir a criação da BD no arranque
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FinVaultInvestmentsDb>();
    // Cria a base de dados e aplica migrações se necessário
    dbContext.Database.EnsureCreated();
}

app.Run();
