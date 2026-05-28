param(
    [string]$ContainerName = "motorcare-staging-postgres",
    [string]$BackupDir = $(if ($env:MOTORCARE_BACKUP_DIR) { $env:MOTORCARE_BACKUP_DIR } else { ".\backups" }),
    [string]$ApiContainerName = "motorcare-staging-api",
    [string]$AttachmentsPath = $(if (${env:Storage__AttachmentsPath}) { ${env:Storage__AttachmentsPath} } else { "/var/lib/motorcare/attachments" })
)

$ErrorActionPreference = "Stop"

& "$PSScriptRoot\backup-motorcare.ps1" `
    -Environment staging `
    -BackupDir $BackupDir `
    -PostgresContainerName $ContainerName `
    -ApiContainerName $ApiContainerName `
    -AttachmentsPath $AttachmentsPath
