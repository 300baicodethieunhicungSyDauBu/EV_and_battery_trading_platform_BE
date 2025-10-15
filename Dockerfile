# Sử dụng .NET SDK 8.0 để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ file vào container
COPY . .

# Restore dependencies
RUN dotnet restore "./EV_and_battery_trading_platform_Be.sln"

# Build toàn bộ solution
RUN dotnet publish "./BE.API/BE.API.csproj" -c Release -o /app/publish

# Runtime stage (chạy app)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "BE.API.dll"]
