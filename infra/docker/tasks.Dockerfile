FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Relatio.Shared/Relatio.Shared.csproj src/Relatio.Shared/
COPY src/Modules/Tasks/Relatio.Tasks.Domain/Relatio.Tasks.Domain.csproj \
     src/Modules/Tasks/Relatio.Tasks.Domain/
COPY src/Modules/Tasks/Relatio.Tasks.Application/Relatio.Tasks.Application.csproj \
     src/Modules/Tasks/Relatio.Tasks.Application/
COPY src/Modules/Tasks/Relatio.Tasks.Infrastructure/Relatio.Tasks.Infrastructure.csproj \
     src/Modules/Tasks/Relatio.Tasks.Infrastructure/
COPY src/Modules/Tasks/Relatio.Tasks.Api/Relatio.Tasks.Api.csproj \
     src/Modules/Tasks/Relatio.Tasks.Api/
COPY src/Modules/Tasks/Relatio.Tasks.Host/Relatio.Tasks.Host.csproj \
     src/Modules/Tasks/Relatio.Tasks.Host/

RUN dotnet restore src/Modules/Tasks/Relatio.Tasks.Host/Relatio.Tasks.Host.csproj

COPY . .
RUN dotnet publish src/Modules/Tasks/Relatio.Tasks.Host/Relatio.Tasks.Host.csproj \
    -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Relatio.Tasks.Host.dll"]
