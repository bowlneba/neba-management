FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5073

ENV ASPNETCORE_URLS=http://+:5073

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["src/Neba.Api/Neba.Api.csproj", "src/Neba.Api/"]
RUN dotnet restore "src/Neba.Api/Neba.Api.csproj"
COPY . .
WORKDIR "/src/src/Neba.Api"
RUN dotnet build "Neba.Api.csproj" -c "$configuration" -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Neba.Api.csproj" -c "$configuration" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Neba.Api.dll"]
