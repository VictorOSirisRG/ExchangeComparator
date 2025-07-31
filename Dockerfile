# Use the official .NET 9.0 runtime image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project files
COPY ["ExchangeComparator.API/ExchangeComparator.API.csproj", "ExchangeComparator.API/"]
COPY ["ExchangeComparator.Application/ExchangeComparator.Application.csproj", "ExchangeComparator.Application/"]
COPY ["ExchangeComparator.Domain/ExchangeComparator.Domain.csproj", "ExchangeComparator.Domain/"]
COPY ["ExchangeComparator.Infrastructure/ExchangeComparator.Infrastructure.csproj", "ExchangeComparator.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "ExchangeComparator.API/ExchangeComparator.API.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/ExchangeComparator.API"
RUN dotnet build "ExchangeComparator.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "ExchangeComparator.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "ExchangeComparator.API.dll"] 