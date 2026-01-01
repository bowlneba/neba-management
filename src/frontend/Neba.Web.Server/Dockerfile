FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Install Node.js for building Tailwind CSS
RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/frontend/Neba.Web.Server/Neba.Web.Server.csproj", "src/frontend/Neba.Web.Server/"]
COPY ["src/frontend/Neba.Web.Client/Neba.Web.Client.csproj", "src/frontend/Neba.Web.Client/"]
COPY ["src/backend/Neba.Website.Contracts/Neba.Website.Contracts.csproj", "src/backend/Neba.Website.Contracts/"]
COPY ["src/shared-kernel/Neba.Contracts/Neba.Contracts.csproj", "src/shared-kernel/Neba.Contracts/"]
RUN dotnet restore "src/frontend/Neba.Web.Server/Neba.Web.Server.csproj"
COPY . .
WORKDIR "/src/src/frontend/Neba.Web.Server"

# Build Tailwind CSS
RUN npm install && npm run build:css

RUN dotnet build "./Neba.Web.Server.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Neba.Web.Server.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish --chown=$APP_UID:$APP_UID /app/publish .
ENTRYPOINT ["dotnet", "Neba.Web.Server.dll"]
