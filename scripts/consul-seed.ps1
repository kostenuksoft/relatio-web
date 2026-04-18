$base = "http://localhost:8500/v1/kv"

$keys = @{
    "relatio/default/RateLimiting"  = '{"RequestsPerWindow":30,"WindowSeconds":60}'
    "relatio/default/ApiVersioning" = '{"DefaultVersion":"1.0"}'
    "relatio/default/Pagination"    = '{"DefaultPageSize":20}'

    "relatio/gateway/RateLimiting"  = '{"RequestsPerWindow":30,"WindowSeconds":60}'

    "relatio/identity/Jwt"          = '{"ExpiryMinutes":60}'
    "relatio/customers/Pagination"  = '{"DefaultPageSize":25}'
    "relatio/sales/Saga"            = '{"CompensationEnabled":"true"}'
    "relatio/tasks/Pagination"      = '{"DefaultPageSize":20}'
}

foreach ($key in $keys.Keys) {
    try {
        Invoke-RestMethod -Uri "$base/$key" -Method PUT -Body $keys[$key] | Out-Null
        write-host "  $key"
    }
    catch {
        write-host "  failed: $key - $($_.Exception.Message)"
    }
}
