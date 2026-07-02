# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Gclopes.FinVault.DocumentIngestion.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime - SEM "slim", com libs completas
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Instalar libs de rede (SSL) e globalização (ICU) essenciais
RUN apt-get update && apt-get install -y --no-install-recommends \
    libicu-dev \
    libssl-dev \
    && rm -rf /var/lib/apt/lists/*

RUN mkdir -p /app/uploadPdf && chown -R app:app /app/uploadPdf
COPY --from=build /app/publish .
USER app
ENTRYPOINT ["dotnet", "Gclopes.FinVault.DocumentIngestion.dll"]