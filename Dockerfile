# ====== STAGE 1: Build the application ======
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Restore dependencies
RUN dotnet restore "EV_and_battery_trading_platform_BE.sln"

# Build project in Release mode
RUN dotnet publish "BE.API/BE.API.csproj" -c Release -o /app/publish

# ====== STAGE 2: Run the application ======
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy build output from previous stage
COPY --from=build /app/publish .

# Expose port (Render sẽ map port này)
EXPOSE 8080

# Environment variable cho ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080

# Start the API
ENTRYPOINT ["dotnet", "BE.API.dll"]
