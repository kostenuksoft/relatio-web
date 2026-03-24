FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Relatio.Shared/Relatio.Shared.csproj src/Relatio.Shared/
COPY src/Modules/Contacts/Relatio.Contacts.Domain/Relatio.Contacts.Domain.csproj \
     src/Modules/Contacts/Relatio.Contacts.Domain/
COPY src/Modules/Contacts/Relatio.Contacts.Application/Relatio.Contacts.Application.csproj \
     src/Modules/Contacts/Relatio.Contacts.Application/
COPY src/Modules/Contacts/Relatio.Contacts.Infrastructure/Relatio.Contacts.Infrastructure.csproj \
     src/Modules/Contacts/Relatio.Contacts.Infrastructure/
COPY src/Modules/Contacts/Relatio.Contacts.Api/Relatio.Contacts.Api.csproj \
     src/Modules/Contacts/Relatio.Contacts.Api/
COPY src/Modules/Contacts/Relatio.Contacts.Host/Relatio.Contacts.Host.csproj \
     src/Modules/Contacts/Relatio.Contacts.Host/

RUN dotnet restore src/Modules/Contacts/Relatio.Contacts.Host/Relatio.Contacts.Host.csproj

COPY . .
RUN dotnet publish src/Modules/Contacts/Relatio.Contacts.Host/Relatio.Contacts.Host.csproj \
    -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Relatio.Contacts.Host.dll"]
