[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("staging", "production")]
    [string]$Environment,

    [string]$BackupDir = $(if ($env:MOTORCARE_BACKUP_DIR) { $env:MOTORCARE_BACKUP_DIR } else { ".\backups" }),

    [string]$ContainerName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ContainerName)) {
    $ContainerName = if ($Environment -eq "staging") {
        "motorcare-staging-postgres"
    }
    else {
        "motorcare-production-postgres"
    }
}

function Invoke-Docker {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)

    & docker @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "docker command failed: docker $($Arguments -join ' ')"
    }
}

$timestamp = (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssZ")
$archiveName = "motorcare-$Environment-$timestamp.dump"
$archivePath = Join-Path $BackupDir $archiveName
$checksumPath = "$archivePath.sha256"
$remotePath = "/tmp/$archiveName"

New-Item -ItemType Directory -Force -Path $BackupDir | Out-Null

try {
    Write-Host "Creating $Environment PostgreSQL backup from $ContainerName"
    Invoke-Docker exec $ContainerName sh -lc "pg_dump -U `"`$POSTGRES_USER`" -d `"`$POSTGRES_DB`" --format=custom --no-owner --no-acl -f '$remotePath'"
    Invoke-Docker exec $ContainerName sh -lc "test -s '$remotePath' && pg_restore --list '$remotePath' >/dev/null"

    Invoke-Docker cp "${ContainerName}:$remotePath" $archivePath

    $archive = Get-Item -LiteralPath $archivePath
    if ($archive.Length -le 0) {
        throw "Backup failed or archive is empty: $archivePath"
    }

    $hash = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $archiveName" | Set-Content -LiteralPath $checksumPath -Encoding ascii

    Write-Host "Checksum written: $checksumPath"
    Write-Host "Backup created: $archivePath ($($archive.Length) bytes)"
}
finally {
    & docker exec $ContainerName rm -f $remotePath 2>$null | Out-Null
}
