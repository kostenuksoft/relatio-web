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

Write-Host "Applying Customers database migrations..." -ForegroundColor Green

dotnet ef database update `
    --project src/Modules/Customers/Relatio.Customers.Infrastructure/Relatio.Customers.Infrastructure.csproj `
    --startup-project Relatio.csproj `
    --context CustomersDbContext

Write-Host "Database updated successfully!" -ForegroundColor Green
