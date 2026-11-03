
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/services/Gclopes.FinVault.MarketPrices/Gclopes.FinVault.MarketPrices.csproj", "src/services/Gclopes.FinVault.MarketPrices/"]

RUN dotnet restore "src/services/Gclopes.FinVault.MarketPrices/Gclopes.FinVault.MarketPrices.csproj"

COPY . .
WORKDIR "/src/src/services/Gclopes.FinVault.MarketPrices"

RUN dotnet publish "Gclopes.FinVault.MarketPrices.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gclopes.FinVault.MarketPrices.dll"]
