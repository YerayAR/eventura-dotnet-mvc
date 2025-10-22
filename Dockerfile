# Use official .NET 9.0 runtime with security updates
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create non-root user for security
RUN addgroup -g 1001 eventura && adduser -D -u 1001 -G eventura eventura

# Install security updates
RUN apk update && apk upgrade && apk add --no-cache \
    curl \
    && rm -rf /var/cache/apk/*

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy csproj files first for better layer caching
COPY ["src/Eventura.Web/Eventura.Web.csproj", "src/Eventura.Web/"]
COPY ["src/Eventura.Application/Eventura.Application.csproj", "src/Eventura.Application/"]
COPY ["src/Eventura.Domain/Eventura.Domain.csproj", "src/Eventura.Domain/"]
COPY ["src/Eventura.Infrastructure/Eventura.Infrastructure.csproj", "src/Eventura.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Eventura.Web/Eventura.Web.csproj"

# Copy source code
COPY . .

# Build and publish
WORKDIR "/src/src/Eventura.Web"
RUN dotnet build "Eventura.Web.csproj" -c Release -o /app/build
RUN dotnet publish "Eventura.Web.csproj" -c Release -o /app/publish --no-restore --verbosity normal

# Final stage
FROM base AS final

# Copy published files
COPY --from=build /app/publish /app

# Set file permissions for security
RUN chown -R eventura:eventura /app
RUN chmod -R 755 /app

# Switch to non-root user
USER eventura

# Add health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV DOTNET_EnableDiagnostics=0

EXPOSE 8080

ENTRYPOINT ["dotnet", "Eventura.Web.dll"]
