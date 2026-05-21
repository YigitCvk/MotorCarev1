[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("staging", "production")]
    [string]$Environment,

    [string]$BackupDir = $(if ($env:MOTORCARE_BACKUP_DIR) { $env:MOTORCARE_BACKUP_DIR } else { ".\backups" }),

    [string]$PostgresContainerName,

    [string]$ApiContainerName,

    [string]$AttachmentsPath = $(if (${env:Storage__AttachmentsPath}) { ${env:Storage__AttachmentsPath} } else { "/var/lib/motorcare/attachments" })
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($PostgresContainerName)) {
    $PostgresContainerName = if ($Environment -eq "staging") { "motorcare-staging-postgres" } else { "motorcare-production-postgres" }
}

if ([string]::IsNullOrWhiteSpace($ApiContainerName)) {
    $ApiContainerName = if ($Environment -eq "staging") { "motorcare-staging-api" } else { "motorcare-production-api" }
}

function Assert-ContainerPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not $Path.StartsWith("/", [StringComparison]::Ordinal)) {
        throw "Container path must be absolute: $Path"
    }

    if ($Path -notmatch '^[A-Za-z0-9_./-]+$') {
        throw "Container path contains unsupported characters: $Path"
    }
}

function Invoke-Docker {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)

    & docker @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "docker command failed: docker $($Arguments -join ' ')"
    }
}

Assert-ContainerPath -Path $AttachmentsPath

$timestamp = (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssZ")
$baseName = "motorcare-$Environment-$timestamp"
$dbArchiveName = "$baseName.dump"
$attachmentsArchiveName = "$baseName-attachments.tar.gz"
$dbArchivePath = Join-Path $BackupDir $dbArchiveName
$attachmentsArchivePath = Join-Path $BackupDir $attachmentsArchiveName
$manifestPath = Join-Path $BackupDir "$baseName.manifest.txt"
$remoteDbPath = "/tmp/$dbArchiveName"
$remoteAttachmentsPath = "/tmp/$attachmentsArchiveName"

New-Item -ItemType Directory -Force -Path $BackupDir | Out-Null

try {
    Write-Host "Creating $Environment PostgreSQL backup from $PostgresContainerName"
    Invoke-Docker exec $PostgresContainerName sh -lc "pg_dump -U `"`$POSTGRES_USER`" -d `"`$POSTGRES_DB`" --format=custom --no-owner --no-acl -f '$remoteDbPath'"
    Invoke-Docker exec $PostgresContainerName sh -lc "test -s '$remoteDbPath' && pg_restore --list '$remoteDbPath' >/dev/null"
    Invoke-Docker cp "${PostgresContainerName}:$remoteDbPath" $dbArchivePath

    $dbArchive = Get-Item -LiteralPath $dbArchivePath
    if ($dbArchive.Length -le 0) {
        throw "Database backup failed or archive is empty: $dbArchivePath"
    }

    Write-Host "Creating $Environment attachment backup from ${ApiContainerName}:$AttachmentsPath"
    Invoke-Docker exec $ApiContainerName sh -lc "if [ -d '$AttachmentsPath' ]; then cd '$AttachmentsPath'; else mkdir -p /tmp/motorcare-empty-attachments && cd /tmp/motorcare-empty-attachments; fi; tar -czf '$remoteAttachmentsPath' ."
    Invoke-Docker exec $ApiContainerName sh -lc "test -s '$remoteAttachmentsPath' && tar -tzf '$remoteAttachmentsPath' >/dev/null"
    Invoke-Docker cp "${ApiContainerName}:$remoteAttachmentsPath" $attachmentsArchivePath

    $attachmentsArchive = Get-Item -LiteralPath $attachmentsArchivePath
    if ($attachmentsArchive.Length -le 0) {
        throw "Attachment backup failed or archive is empty: $attachmentsArchivePath"
    }

    $dbHash = (Get-FileHash -LiteralPath $dbArchivePath -Algorithm SHA256).Hash.ToLowerInvariant()
    "$dbHash  $dbArchiveName" | Set-Content -LiteralPath "$dbArchivePath.sha256" -Encoding ascii

    $attachmentsHash = (Get-FileHash -LiteralPath $attachmentsArchivePath -Algorithm SHA256).Hash.ToLowerInvariant()
    "$attachmentsHash  $attachmentsArchiveName" | Set-Content -LiteralPath "$attachmentsArchivePath.sha256" -Encoding ascii

    @(
        "environment=$Environment",
        "created_utc=$timestamp",
        "postgres_container=$PostgresContainerName",
        "api_container=$ApiContainerName",
        "attachments_path=$AttachmentsPath",
        "database_archive=$dbArchiveName",
        "attachments_archive=$attachmentsArchiveName"
    ) | Set-Content -LiteralPath $manifestPath -Encoding ascii

    Write-Host "Database backup created: $dbArchivePath ($($dbArchive.Length) bytes)"
    Write-Host "Attachment backup created: $attachmentsArchivePath ($($attachmentsArchive.Length) bytes)"
    Write-Host "Manifest written: $manifestPath"
}
finally {
    & docker exec $PostgresContainerName rm -f $remoteDbPath 2>$null | Out-Null
    & docker exec $ApiContainerName rm -f $remoteAttachmentsPath 2>$null | Out-Null
}
