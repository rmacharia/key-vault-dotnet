param(
    [string]$TargetDir,
    [string]$SecretValue
)

# Stop existing app
Stop-Process -Name "key-vault-dotnet-core-quickstart" -ErrorAction SilentlyContinue

# Backup existing app (optional)
$backupPath = "$TargetDir\backup_$(Get-Date -Format 'yyyyMMddHHmmss')"
Copy-Item -Path $TargetDir -Destination $backupPath -Recurse -Force -ErrorAction SilentlyContinue

# Copy new files from build artifacts
Copy-Item -Path "$(Pipeline.Workspace)/drop" -Destination $TargetDir -Recurse -Force

# Update appsettings.json with Key Vault URL (if needed)
$appSettingsPath = Join-Path $TargetDir "appsettings.json"
if (Test-Path $appSettingsPath) {
    $appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
    $appSettings.KeyVaultUrl = "https://rgvault254.vault.azure.net/" # Your Key Vault URL
    $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
}

# Start the app
Start-Process "dotnet" -ArgumentList "run" -WorkingDirectory $TargetDir

Write-Host "Deployment to $TargetDir completed"