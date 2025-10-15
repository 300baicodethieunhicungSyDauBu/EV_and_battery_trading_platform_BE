# ===============================
# Stage 1: Build
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file trước
COPY EV_and_battery_trading_platform_BE.sln ./

# Copy các project folder riêng (đảm bảo cấu trúc)
COPY BE.API ./BE.API
COPY BE.BOs ./BE.BOs
COPY BE.DAOs ./BE.DAOs
COPY BE.REPOs ./BE.REPOs

# Restore dependencies
RUN dotnet restore "EV_and_battery_trading_platform_BE.sln"

# Build in Release mode
RUN dotnet publish "BE.API/BE.API.csproj" -c Release -o /app/publish /p:UseAppHost=false


# ===============================
# Stage 2: Runtime
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BE.API.dll"]
