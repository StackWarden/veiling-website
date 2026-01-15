# BUILD
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY backend.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . .
# Ensure .env file is available (even if it might be in .dockerignore, we want it lol)
COPY .env* ./
RUN dotnet build -c Release -o /app/build

# PUBLISH
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# RUNTIME
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Copy .env file from build context
# This allows the app to load .env file at runtime
COPY --from=build /src/.env* ./

# Expose port
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "backend.dll"]
