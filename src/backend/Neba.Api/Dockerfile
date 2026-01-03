FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/backend/Neba.Api/Neba.Api.csproj", "src/backend/Neba.Api/"]
COPY ["src/backend/Neba.Application/Neba.Application.csproj", "src/backend/Neba.Application/"]
COPY ["src/backend/Neba.Domain/Neba.Domain.csproj", "src/backend/Neba.Domain/"]
COPY ["src/backend/Neba.Infrastructure/Neba.Infrastructure.csproj", "src/backend/Neba.Infrastructure/"]
RUN dotnet restore "src/backend/Neba.Api/Neba.Api.csproj"
COPY . .
WORKDIR "/src/src/backend/Neba.Api"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Neba.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Neba.Api.dll"]
