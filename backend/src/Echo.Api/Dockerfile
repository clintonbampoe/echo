
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /build

COPY ["backend/Echo.slnx", "."]
COPY ["backend/Directory.Packages.props", "."]
COPY ["backend/src/Echo.Api/Echo.Api.csproj", "./src/Echo.Api/"]
COPY ["backend/src/Echo.Application/Echo.Application.csproj", "./src/Echo.Application/"]
COPY ["backend/src/Echo.Auth/Echo.Auth.csproj", "./src/Echo.Auth/"]
COPY ["backend/src/Echo.Shared/Echo.Shared.csproj", "./src/Echo.Shared/"]
COPY ["backend/src/Echo.Domain/Echo.Domain.csproj", "./src/Echo.Domain/"]
COPY ["backend/src/Echo.Data/Echo.Data.csproj", "./src/Echo.Data/"]
COPY ["backend/src/Echo.ServiceDefaults/Echo.ServiceDefaults.csproj", "./src/Echo.ServiceDefaults/"]
COPY "backend/.config/dotnet-tools.json" "/backend/.config/"

RUN dotnet restore src/Echo.Api/Echo.Api.csproj

FROM restore AS build
COPY backend/ /build

RUN dotnet build src/Echo.Api/Echo.Api.csproj -c Release --no-restore

FROM build AS publish
RUN dotnet publish src/Echo.Api/Echo.Api.csproj \
  -c Release \
  --no-build \
  -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Curl is installed on runtime image to enable health checks
RUN apt-get update \
  && apt-get install -y --no-install-recommends curl \
  && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:5025

ENTRYPOINT [ "dotnet", "Echo.Api.dll" ]
