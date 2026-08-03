
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /backend

COPY ["backend/Echo.slnx", "."]
COPY ["backend/Directory.Packages.props", "."]
COPY ["backend/src/Echo.Api/Echo.Api.csproj", "./src/Echo.Api/"]
COPY ["backend/src/Echo.Application/Echo.Application.csproj", "./src/Echo.Application/"]
COPY ["backend/src/Echo.Auth/Echo.Auth.csproj", "./src/Echo.Auth/"]
COPY ["backend/src/Echo.Core/Echo.Core.csproj", "./src/Echo.Core/"]
COPY ["backend/src/Echo.Domain/Echo.Domain.csproj", "./src/Echo.Domain/"]
COPY ["backend/src/Echo.Infrastructure/Echo.Infrastructure.csproj", "./src/Echo.Infrastructure/"]

RUN dotnet restore src/Echo.Api/Echo.Api.csproj

FROM restore AS build
COPY backend/ .

RUN dotnet tool restore
RUN dotnet build src/Echo.Api/Echo.Api.csproj -c Release --no-restore

# Install curl for health checks
RUN apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM build AS publish
RUN dotnet publish src/Echo.Api/Echo.Api.csproj \
    -c Release \
    --no-build \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

ENTRYPOINT [ "dotnet", "Echo.Api.dll" ]
