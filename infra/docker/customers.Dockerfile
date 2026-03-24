FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Relatio.Shared/Relatio.Shared.csproj src/Relatio.Shared/
COPY src/Modules/Customers/Relatio.Customers.Domain/Relatio.Customers.Domain.csproj \
     src/Modules/Customers/Relatio.Customers.Domain/
COPY src/Modules/Customers/Relatio.Customers.Application/Relatio.Customers.Application.csproj \
     src/Modules/Customers/Relatio.Customers.Application/
COPY src/Modules/Customers/Relatio.Customers.Infrastructure/Relatio.Customers.Infrastructure.csproj \
     src/Modules/Customers/Relatio.Customers.Infrastructure/
COPY src/Modules/Customers/Relatio.Customers.Api/Relatio.Customers.Api.csproj \
     src/Modules/Customers/Relatio.Customers.Api/
COPY src/Modules/Customers/Relatio.Customers.Host/Relatio.Customers.Host.csproj \
     src/Modules/Customers/Relatio.Customers.Host/

RUN dotnet restore src/Modules/Customers/Relatio.Customers.Host/Relatio.Customers.Host.csproj

COPY . .
RUN dotnet publish src/Modules/Customers/Relatio.Customers.Host/Relatio.Customers.Host.csproj \
    -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Relatio.Customers.Host.dll"]
