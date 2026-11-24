# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["global.json", "./"]
COPY ["src/tools/Gclopes.FinVault.DatabaseMigrator/Gclopes.FinVault.DatabaseMigrator.csproj","src/tools/Gclopes.FinVault.DatabaseMigrator/"]
COPY ["src/services/Gclopes.FinVault.DocumentIngestion/Gclopes.FinVault.DocumentIngestion.csproj","src/services/Gclopes.FinVault.DocumentIngestion/"]
COPY ["src/services/Gclopes.FinVault.Identity/Gclopes.FinVault.Identity.csproj","src/services/Gclopes.FinVault.Identity/"]
COPY ["src/services/Gclopes.FinVault.CurrentAccount/Gclopes.FinVault.CurrentAccount.csproj","src/services/Gclopes.FinVault.CurrentAccount/"]
COPY ["src/services/Gclopes.FinVault.Investments/Gclopes.FinVault.Investments.csproj","src/services/Gclopes.FinVault.Investments/"]
COPY ["src/services/Gclopes.FinVault.PensionPlans/Gclopes.FinVault.PensionPlans.csproj","src/services/Gclopes.FinVault.PensionPlans/"]
COPY ["src/services/Gclopes.FinVault.SavingsCertificates/Gclopes.FinVault.SavingsCertificates.csproj","src/services/Gclopes.FinVault.SavingsCertificates/"]
RUN dotnet restore "./src/tools/Gclopes.FinVault.DatabaseMigrator/Gclopes.FinVault.DatabaseMigrator.csproj"
COPY . .
WORKDIR "/src/src/tools/Gclopes.FinVault.DatabaseMigrator"
RUN dotnet build "./Gclopes.FinVault.DatabaseMigrator.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Gclopes.FinVault.DatabaseMigrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gclopes.FinVault.DatabaseMigrator.dll"]