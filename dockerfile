# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /workBuild

COPY ["src/Botify.API/Botify.API.csproj", "src/Botify.API/"]
COPY ["src/Botify.Application/Botify.Application.csproj", "src/Botify.Application/"]
COPY ["src/Botify.Domain/Botify.Domain.csproj", "src/Botify.Domain/"]
COPY ["src/Botify.Infrastructure/Botify.Infrastructure.csproj", "src/Botify.Infrastructure/"]

RUN dotnet restore "src/Botify.API/Botify.API.csproj"

COPY . .

WORKDIR "/workBuild/src/Botify.API"
RUN dotnet publish "Botify.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# إنشاء مستخدم غير root (بدون تحديد UID)
RUN useradd -m appuser
RUN mkdir -p /app/wwwroot && chown -R appuser:appuser /app

USER appuser

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Botify.API.dll"]
