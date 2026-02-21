$secret = [Convert]::ToBase64String((1..64 | ForEach-Object { [byte](Get-Random -Max 256) }))
Write-Host "JWT Secret:" -ForegroundColor Green
Write-Host $secret
Write-Host ""
Write-Host "Add to appsettings.Development.json:" -ForegroundColor Yellow
Write-Host """Jwt"": { ""Secret"": ""$secret"" }"
