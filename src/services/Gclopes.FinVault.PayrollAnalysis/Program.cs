var builder = WebApplication.CreateBuilder(args);

// Just a dummy entry point to satisfy the compiler
var app = builder.Build();

app.MapGet("/", () => "Service is under construction...");

app.Run();