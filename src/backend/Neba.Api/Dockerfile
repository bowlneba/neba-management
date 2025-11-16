FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["../../Directory.Build.props", "./"]
COPY ["../../Directory.Packages.props", "./"]
COPY ["Neba.Api/Neba.Api.csproj", "Neba.Api/"]
COPY ["Neba.Application/Neba.Application.csproj", "Neba.Application/"]
COPY ["Neba.Domain/Neba.Domain.csproj", "Neba.Domain/"]
COPY ["Neba.Infrastructure/Neba.Infrastructure.csproj", "Neba.Infrastructure/"]
RUN dotnet restore "Neba.Api/Neba.Api.csproj"
COPY . .
WORKDIR "/src/Neba.Api"
RUN dotnet build "./Neba.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Neba.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Neba.Api.dll"]
