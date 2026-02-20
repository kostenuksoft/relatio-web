param(
    [string]$MigrationName = "InitialCreate"
)

if (-Not (Test-Path .env)) {
    Write-Host "Error: .env file not found in root directory" -ForegroundColor Red
    exit 1
}

Get-Content .env | ForEach-Object {
    if ($_ -match '^(.+)=(.+)$') {
        [System.Environment]::SetEnvironmentVariable($matches[1], $matches[2])
    }
}

$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=$($env:DB_PORT);Database=$($env:DB_NAME);Username=$($env:DB_USER);Password=$($env:DB_PASSWORD)"

Write-Host "Running Customers migration: $MigrationName" -ForegroundColor Green

dotnet ef migrations add $MigrationName `
    --project src/Modules/Customers/Relatio.Customers.Infrastructure/Relatio.Customers.Infrastructure.csproj `
    --startup-project Relatio.csproj `
    --context CustomersDbContext

Write-Host "Migration '$MigrationName' created successfully!" -ForegroundColor Green
