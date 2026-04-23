# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy and restore (separated for faster caching)
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and publish
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# 2. Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy the published files from the build stage
COPY --from=build /app/publish .

# GCP Cloud Run requirement: Listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# CRITICAL: Match the casing of your DLL exactly!
ENTRYPOINT ["dotnet", "PetWise-API.dll"]