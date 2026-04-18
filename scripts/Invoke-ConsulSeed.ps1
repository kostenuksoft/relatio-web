$Base = "http://localhost:8500/v1/kv"

$Keys = @{
    "relatio/default/RateLimiting"  = '{"RequestsPerWindow":30,"WindowSeconds":60}'
    "relatio/default/ApiVersioning" = '{"DefaultVersion":"1.0"}'
    "relatio/default/Pagination"    = '{"DefaultPageSize":20}'

    "relatio/development/Logging"   = '{"MinimumLevel":"Debug"}'

    "relatio/gateway/RateLimiting"  = '{"RequestsPerWindow":30,"WindowSeconds":60}'

    "relatio/identity/Jwt"          = '{"ExpiryMinutes":60}'
    "relatio/customers/Pagination"  = '{"DefaultPageSize":25}'
    "relatio/sales/Saga"            = '{"CompensationEnabled":"true"}'
    "relatio/tasks/Pagination"      = '{"DefaultPageSize":20}'
}

foreach ($Key in $Keys.Keys) {
    try {
        Invoke-RestMethod -Uri "$Base/$Key" -Method PUT -Body $Keys[$Key] | Out-Null
        write-host "  $Key"
    }
    catch {
        write-host "  failed: $Key - $($_.Exception.Message)"
    }
}
