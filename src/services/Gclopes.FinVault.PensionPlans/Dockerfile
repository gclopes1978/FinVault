# Base runtime image for .NET 9 REST APIs
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# SDK image used to build the application binaries
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the csproj file using the global context path mapping
COPY ["src/services/Gclopes.FinVault.PensionPlans/Gclopes.FinVault.PensionPlans.csproj", "src/services/Gclopes.FinVault.PensionPlans/"]

# Restore NuGet dependencies
RUN dotnet restore "src/services/Gclopes.FinVault.PensionPlans/Gclopes.FinVault.PensionPlans.csproj"

# Copy the rest of the application source code files
COPY . .
WORKDIR "/src/src/services/Gclopes.FinVault.PensionPlans"

# Compile the application project in Release mode
RUN dotnet build "Gclopes.FinVault.PensionPlans.csproj" -c Release -o /app/build

# Publish the compiled assembly to a deployment folder
FROM build AS publish
RUN dotnet publish "Gclopes.FinVault.PensionPlans.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final layer: bundle everything together using the runtime base environment
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gclopes.FinVault.PensionPlans.dll"]
