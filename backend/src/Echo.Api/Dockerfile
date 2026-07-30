# ---- restore: csprojs only, no source. ----
FROM mcr.microsoft.com/dotnet/sdk:10.0.302 AS restore
WORKDIR /src

# 1. Tell NuGet to store packages in a location we control (inside the build context)
ENV NUGET_PACKAGES=/src/.nuget/packages

COPY backend/Directory.Packages.props backend/Echo.slnx ./backend/
COPY backend/src/Echo.Api/Echo.Api.csproj              ./backend/src/Echo.Api/
COPY backend/src/Echo.Application/Echo.Application.csproj ./backend/src/Echo.Application/
COPY backend/src/Echo.Auth/Echo.Auth.csproj             ./backend/src/Echo.Auth/
COPY backend/src/Echo.Core/Echo.Core.csproj             ./backend/src/Echo.Core/
COPY backend/src/Echo.Domain/Echo.Domain.csproj         ./backend/src/Echo.Domain/
COPY backend/src/Echo.Infrastructure/Echo.Infrastructure.csproj ./backend/src/Echo.Infrastructure/

# Restore: packages go into /src/.nuget/packages
RUN dotnet restore backend/src/Echo.Api/Echo.Api.csproj

# ---- compile: csprojs + source + NuGet cache. Dev stops here. ----
FROM mcr.microsoft.com/dotnet/sdk:10.0.302 AS compile
WORKDIR /src

# 2. Copy the NuGet cache from the restore stage
COPY --from=restore /src/.nuget/packages ./nuget/packages
ENV NUGET_PACKAGES=/src/nuget/packages

# Copy restored project files (including obj/project.assets.json)
COPY --from=restore /src/backend ./backend
COPY backend/src ./backend/src

RUN dotnet build backend/src/Echo.Api/Echo.Api.csproj \
    -c Release \
    --no-restore \
    -o /app/build

# ---- publish: same source + cache, runs publish. Prod needs this. ----
FROM mcr.microsoft.com/dotnet/sdk:10.0.302 AS publish
WORKDIR /src

# 3. Same as compile – copy NuGet cache and restored files
COPY --from=restore /src/.nuget/packages ./nuget/packages
ENV NUGET_PACKAGES=/src/nuget/packages

COPY --from=restore /src/backend ./backend
COPY backend/src ./backend/src

RUN dotnet publish backend/src/Echo.Api/Echo.Api.csproj \
    -c Release \
    --no-restore \
    -o /app/publish

# ---- runtime: minimal, only the publish output. ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0.10 AS runtime
WORKDIR /app

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Echo.Api.dll"]
