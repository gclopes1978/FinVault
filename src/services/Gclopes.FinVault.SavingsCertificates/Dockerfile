# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["Gclopes.FinVault.SavingsCertificates.csproj", "./"]
RUN dotnet restore "Gclopes.FinVault.SavingsCertificates.csproj"

# Copy all source code and build
COPY . .
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