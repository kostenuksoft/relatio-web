FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Relatio.Shared/Relatio.Shared.csproj src/Relatio.Shared/
COPY src/Modules/Sales/Relatio.Sales.Domain/Relatio.Sales.Domain.csproj \
     src/Modules/Sales/Relatio.Sales.Domain/
COPY src/Modules/Sales/Relatio.Sales.Application/Relatio.Sales.Application.csproj \
     src/Modules/Sales/Relatio.Sales.Application/
COPY src/Modules/Sales/Relatio.Sales.Infrastructure/Relatio.Sales.Infrastructure.csproj \
     src/Modules/Sales/Relatio.Sales.Infrastructure/
COPY src/Modules/Sales/Relatio.Sales.Api/Relatio.Sales.Api.csproj \
     src/Modules/Sales/Relatio.Sales.Api/
COPY src/Modules/Sales/Relatio.Sales.Host/Relatio.Sales.Host.csproj \
     src/Modules/Sales/Relatio.Sales.Host/

RUN dotnet restore src/Modules/Sales/Relatio.Sales.Host/Relatio.Sales.Host.csproj

COPY . .
RUN dotnet publish src/Modules/Sales/Relatio.Sales.Host/Relatio.Sales.Host.csproj \
    -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Relatio.Sales.Host.dll"]
