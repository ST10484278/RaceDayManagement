# Runtime image stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# SDK image stage for restoring and building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first for layer caching
COPY RaceDay.Mvc/RaceDay.Mvc.csproj RaceDay.Mvc/RaceDay.Mvc.csproj
COPY RaceDay.Contracts/RaceDay.Contracts.csproj RaceDay.Contracts/RaceDay.Contracts.csproj
RUN dotnet restore RaceDay.Mvc/RaceDay.Mvc.csproj

# Copy remaining repository files
COPY . .
WORKDIR /src/RaceDay.Mvc
RUN dotnet build RaceDay.Mvc.csproj -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish RaceDay.Mvc.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]