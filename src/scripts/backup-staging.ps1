param(
    [string]$ContainerName = "motorcare-staging-postgres",
    [string]$BackupDir = ".\backups"
)

$ErrorActionPreference = "Stop"

$timestamp = (Get-Date).ToUniversalTime().ToString("yyyyMMdd_HHmmss")
$remoteFile = "/tmp/motorcare_staging_backup.dump"
$localFile = Join-Path $BackupDir "motorcare_staging_backup_$timestamp.dump"

New-Item -ItemType Directory -Force -Path $BackupDir | Out-Null

Write-Host "Creating staging backup from container: $ContainerName"
docker exec $ContainerName sh -c 'pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" -F c -f /tmp/motorcare_staging_backup.dump'

Write-Host "Copying backup to: $localFile"
docker cp "${ContainerName}:${remoteFile}" $localFile

$backup = Get-Item -LiteralPath $localFile
if ($backup.Length -le 0) {
    throw "Backup failed or file is empty: $localFile"
}

Write-Host "Backup created successfully: $localFile ($($backup.Length) bytes)"

