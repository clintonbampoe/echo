FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore 
WORKDIR /app

COPY ["backend/Echo.slnx", "."]
COPY ["backend/Directory.Packages.props", "."]
COPY ["backend/src/Echo.MigrationService/Echo.MigrationService.csproj", "./src/Echo.MigrationService/"]
COPY ["backend/src/Echo.Data/Echo.Data.csproj", "./src/Echo.Data/"]
COPY ["backend/src/Echo.Domain/Echo.Domain.csproj", "./src/Echo.Domain/"]
COPY ["backend/src/Echo.Shared/Echo.Shared.csproj", "./src/Echo.Shared/"]
COPY ["backend/src/Echo.ServiceDefaults/Echo.ServiceDefaults.csproj", "./src/Echo.ServiceDefaults/"]

RUN dotnet restore src/Echo.MigrationService/Echo.MigrationService.csproj

FROM restore AS build
COPY backend/ .

RUN dotnet publish src/Echo.MigrationService/Echo.MigrationService.csproj --no-restore -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Echo.MigrationService.dll"]
