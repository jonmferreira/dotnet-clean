FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

RUN dotnet tool install --global dotnet-ef --version 8.0.8
ENV PATH="$PATH:/root/.dotnet/tools"

WORKDIR /app

COPY src/Parking.Api/Parking.Api.csproj src/Parking.Api/
COPY src/Parking.Application/Parking.Application.csproj src/Parking.Application/
COPY src/Parking.Domain/Parking.Domain.csproj src/Parking.Domain/
COPY src/Parking.Infrastructure/Parking.Infrastructure.csproj src/Parking.Infrastructure/

RUN dotnet restore src/Parking.Api/Parking.Api.csproj

COPY . .

RUN dotnet publish src/Parking.Api/Parking.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Parking.Api.dll"]
