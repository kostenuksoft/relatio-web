FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Relatio.Shared/Relatio.Shared.csproj src/Relatio.Shared/
COPY src/Modules/Identity/Relatio.Identity.Domain/Relatio.Identity.Domain.csproj \
     src/Modules/Identity/Relatio.Identity.Domain/
COPY src/Modules/Identity/Relatio.Identity.Application/Relatio.Identity.Application.csproj \
     src/Modules/Identity/Relatio.Identity.Application/
COPY src/Modules/Identity/Relatio.Identity.Infrastructure/Relatio.Identity.Infrastructure.csproj \
     src/Modules/Identity/Relatio.Identity.Infrastructure/
COPY src/Modules/Identity/Relatio.Identity.Api/Relatio.Identity.Api.csproj \
     src/Modules/Identity/Relatio.Identity.Api/
COPY src/Modules/Identity/Relatio.Identity.Host/Relatio.Identity.Host.csproj \
     src/Modules/Identity/Relatio.Identity.Host/

RUN dotnet restore src/Modules/Identity/Relatio.Identity.Host/Relatio.Identity.Host.csproj

COPY . .
RUN dotnet publish src/Modules/Identity/Relatio.Identity.Host/Relatio.Identity.Host.csproj \
    -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Relatio.Identity.Host.dll"]
