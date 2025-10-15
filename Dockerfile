# ===============================
# Stage 1: Build
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all files
COPY . .

# Restore dependencies
RUN dotnet restore "./EV_and_battery_trading_platform_BE.sln"

# Build in Release mode
RUN dotnet publish "BE.API/BE.API.csproj" -c Release -o /app/publish /p:UseAppHost=false


# ===============================
# Stage 2: Runtime
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Expose default port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BE.API.dll"]
