FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# Use an official Node image to build frontend assets (avoids executing third-party install scripts)
FROM node:20-bullseye-slim AS nodebuild
WORKDIR /src

# Copy frontend projects for npm build (keeps the stage small and focused)
COPY src/frontend/Neba.Web.Server/ src/frontend/Neba.Web.Server/
COPY src/frontend/Neba.Web.Client/ src/frontend/Neba.Web.Client/

WORKDIR /src/src/frontend/Neba.Web.Server
# Use npm install to build Tailwind CSS assets
RUN npm install && npm run build:css

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/frontend/Neba.Web.Server/Neba.Web.Server.csproj", "src/frontend/Neba.Web.Server/"]
COPY ["src/frontend/Neba.Web.Client/Neba.Web.Client.csproj", "src/frontend/Neba.Web.Client/"]
COPY ["src/backend/Neba.Website.Contracts/Neba.Website.Contracts.csproj", "src/backend/Neba.Website.Contracts/"]
COPY ["src/shared-kernel/Neba.Contracts/Neba.Contracts.csproj", "src/shared-kernel/Neba.Contracts/"]
RUN dotnet restore "src/frontend/Neba.Web.Server/Neba.Web.Server.csproj"

# Copy the full repo and then overlay built frontend artifacts from the node build stage
COPY . .
COPY --from=nodebuild /src/src/frontend/Neba.Web.Server/wwwroot /src/src/frontend/Neba.Web.Server/wwwroot

WORKDIR "/src/src/frontend/Neba.Web.Server"
RUN dotnet build "./Neba.Web.Server.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Neba.Web.Server.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish --chown=$APP_UID:$APP_UID /app/publish .
ENTRYPOINT ["dotnet", "Neba.Web.Server.dll"]
