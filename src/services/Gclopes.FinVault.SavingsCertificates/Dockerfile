# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# FIX: docker-compose.yml builds this service with "context: ." (the repo root),
# same as the other microservices, but these COPY paths assumed the build context
# was this service's own folder. That mismatch made "docker compose build" fail
# with "file not found", since the csproj doesn't exist at the repo root.
# Paths below are now relative to the repo root, matching the other services
# (e.g. Gclopes.FinVault.PensionPlans/Dockerfile).
COPY ["src/services/Gclopes.FinVault.SavingsCertificates/Gclopes.FinVault.SavingsCertificates.csproj", "src/services/Gclopes.FinVault.SavingsCertificates/"]
RUN dotnet restore "src/services/Gclopes.FinVault.SavingsCertificates/Gclopes.FinVault.SavingsCertificates.csproj"

# Copy all source code and build
COPY . .
WORKDIR "/src/src/services/Gclopes.FinVault.SavingsCertificates"
RUN dotnet build "Gclopes.FinVault.SavingsCertificates.csproj" -c Release -o /app/build

# Stage 2: Publish the application
FROM build AS publish
RUN dotnet publish "Gclopes.FinVault.SavingsCertificates.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy published files from build stage
COPY --from=publish /app/publish .

# Set entry point
ENTRYPOINT ["dotnet", "Gclopes.FinVault.SavingsCertificates.dll"]
