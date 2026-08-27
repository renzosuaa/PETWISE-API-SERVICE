FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["PetWise-API/PetWise-API.csproj", "PetWise-API/"]
COPY ["PetWise-Application/PetWise-Application.csproj", "PetWise-Application/"]
COPY ["PetWise-Infrastructure/PetWise-Infrastructure.csproj", "PetWise-Infrastructure/"]
COPY ["PetWise-Domain/PetWise-Domain.csproj", "PetWise-Domain/"]

RUN dotnet restore "PetWise-API/PetWise-API.csproj"

COPY . .

WORKDIR "/src/PetWise-API"
RUN dotnet publish "PetWise-API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PetWise-API.dll"]