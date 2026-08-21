# Build context is the repository root (see docker-compose.yml), so that this
# Dockerfile can reach the project references (Shared.Contracts).

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SearchService.Api/SearchService.Api.csproj", "SearchService.Api/"]
COPY ["Shared.Contracts/Shared.Contracts.csproj", "Shared.Contracts/"]
RUN dotnet restore "SearchService.Api/SearchService.Api.csproj"
COPY . .
WORKDIR "/src/SearchService.Api"
RUN dotnet build "SearchService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SearchService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SearchService.Api.dll"]
