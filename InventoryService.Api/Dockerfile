# Build context is the repository root (see docker-compose.yml), so that this
# Dockerfile can reach the project references (Shared.Contracts, *.Infrastructure).

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["InventoryService.Api/InventoryService.Api.csproj", "InventoryService.Api/"]
COPY ["InventoryService.Infrastructure/InventoryService.Infrastructure.csproj", "InventoryService.Infrastructure/"]
COPY ["Shared.Contracts/Shared.Contracts.csproj", "Shared.Contracts/"]
RUN dotnet restore "InventoryService.Api/InventoryService.Api.csproj"
COPY . .
WORKDIR "/src/InventoryService.Api"
RUN dotnet build "InventoryService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InventoryService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventoryService.Api.dll"]
