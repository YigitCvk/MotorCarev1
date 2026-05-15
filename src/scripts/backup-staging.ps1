param(
    [string]$ContainerName = "motorcare-staging-postgres",
    [string]$BackupDir = $(if ($env:MOTORCARE_BACKUP_DIR) { $env:MOTORCARE_BACKUP_DIR } else { ".\backups" })
)

$ErrorActionPreference = "Stop"

& "$PSScriptRoot\backup-postgres.ps1" -Environment staging -BackupDir $BackupDir -ContainerName $ContainerName
