# syntax=docker/dockerfile:1.7-labs
#
# Build context is expected to be the repo root, not this folder.
# The paths below assume that.
# Build with: docker build -f backend/src/Echo.Api/Dockerfile .

# Build stage pulls the full SDK image, discarded after publish.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only project/package files first so `dotnet restore` is cached
# and only reruns when a dependency actually changes, not on every
# source edit. --parents preserves each file's folder structure
# instead of needing one COPY line per project.
COPY --parents backend/Directory.Packages.props \
               backend/Echo.slnx \
               backend/src/*/*.csproj ./

RUN dotnet restore backend/src/Echo.Api/Echo.Api.csproj

COPY backend/src ./backend/src

RUN dotnet publish backend/src/Echo.Api/Echo.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Echo.Api.dll"]
