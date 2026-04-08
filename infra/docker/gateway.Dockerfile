FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Relatio.Gateway/Relatio.Gateway.csproj src/Relatio.Gateway/

RUN dotnet restore src/Relatio.Gateway/Relatio.Gateway.csproj

COPY . .
RUN dotnet publish src/Relatio.Gateway/Relatio.Gateway.csproj \
    -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Relatio.Gateway.dll"]
