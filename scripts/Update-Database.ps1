param(
    [Parameter(Mandatory=$true)]
    [string]$Module
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

$validModules = @("Customers", "Contacts", "Sales", "Tasks", "Identity")
if ($validModules -notcontains $Module) {
    Write-Host "Error: Invalid module. Valid modules: $($validModules -join ', ')" -ForegroundColor Red
    exit 1
}

if ($Module -eq "Identity") {
    $contextName = "AuthDbContext"
} else {
    $contextName = "$($Module)DbContext"
}

$projectPath = "src/Modules/$Module/Relatio.$Module.Infrastructure/Relatio.$Module.Infrastructure.csproj"

if (-Not (Test-Path $projectPath)) {
    Write-Host "Error: Project not found at $projectPath" -ForegroundColor Red
    exit 1
}

$hostPath = "src/Modules/$Module/Relatio.$Module.Host/Relatio.$Module.Host.csproj"
$startupProject = if (Test-Path $hostPath) { $hostPath } else { "Relatio.csproj" }

Write-Host "Applying $Module database migrations (context: $contextName)..." -ForegroundColor Green

dotnet ef database update `
    --project $projectPath `
    --startup-project $startupProject `
    --context $contextName

if ($LASTEXITCODE -eq 0) {
    Write-Host "Database updated successfully!" -ForegroundColor Green
} else {
    Write-Host "Database update failed!" -ForegroundColor Red
    exit 1
}
