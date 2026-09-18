
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /backend

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

COPY ["backend/Echo.slnx", "."]
COPY ["backend/Directory.Packages.props", "."]
COPY ["backend/src/Echo.Api/Echo.Api.csproj", "./src/Echo.Api/"]
COPY ["backend/src/Echo.Application/Echo.Application.csproj", "./src/Echo.Application/"]
COPY ["backend/src/Echo.Auth/Echo.Auth.csproj", "./src/Echo.Auth/"]
COPY ["backend/src/Echo.Core/Echo.Core.csproj", "./src/Echo.Core/"]
COPY ["backend/src/Echo.Domain/Echo.Domain.csproj", "./src/Echo.Domain/"]
COPY ["backend/src/Echo.Infrastructure/Echo.Infrastructure.csproj", "./src/Echo.Infrastructure/"]
COPY "backend/.config/dotnet-tools.json" "/backend/.config/"

RUN dotnet restore src/Echo.Api/Echo.Api.csproj

FROM restore AS build
COPY backend/ .

RUN dotnet tool restore
RUN dotnet build src/Echo.Api/Echo.Api.csproj -c Release --no-restore

# curl is required by the api healthcheck in docker-compose.yml. The dev override
# (docker-compose.override.yml) builds the api service from this stage, so it needs
# curl too -- installing it only in the runtime stage would break the dev stack.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

FROM build AS publish
RUN dotnet publish src/Echo.Api/Echo.Api.csproj \
    -c Release \
    --no-build \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# The runtime image is a fresh aspnet base -- packages from the build stage are not
# carried over, so curl must be installed again for the healthcheck to work in prod.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

ENTRYPOINT [ "dotnet", "Echo.Api.dll" ]
