[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("staging", "production")]
    [string]$Environment,

    [Parameter(Mandatory = $true)]
    [string]$BackupFile,

    [string]$TargetDatabase = "motorcare_restore_check",

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

if (-not (Test-Path -LiteralPath $BackupFile -PathType Leaf)) {
    throw "Backup file does not exist: $BackupFile"
}

$backup = Get-Item -LiteralPath $BackupFile
if ($backup.Length -le 0) {
    throw "Backup file is empty: $BackupFile"
}

if ($TargetDatabase -notmatch '^[A-Za-z_][A-Za-z0-9_]*$') {
    throw "Unsafe target database name: $TargetDatabase"
}

function Invoke-Docker {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)

    & docker @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "docker command failed: docker $($Arguments -join ' ')"
    }
}

$sourceDatabase = (& docker exec $ContainerName sh -lc 'printf "%s" "$POSTGRES_DB"').Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($sourceDatabase)) {
    throw "Could not determine live POSTGRES_DB from $ContainerName"
}

if ($TargetDatabase -eq $sourceDatabase) {
    throw "Refusing to restore into live database: $TargetDatabase"
}

$timestamp = (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssZ")
$remotePath = "/tmp/motorcare-restore-$timestamp.dump"

try {
    Write-Host "Copying backup archive into $ContainerName for validation"
    Invoke-Docker cp $BackupFile "${ContainerName}:$remotePath"
    Invoke-Docker exec $ContainerName sh -lc "test -s '$remotePath' && pg_restore --list '$remotePath' >/dev/null"

    Write-Host "Recreating restore-check database: $TargetDatabase"
    Invoke-Docker exec $ContainerName sh -lc "dropdb -U `"`$POSTGRES_USER`" --if-exists --force '$TargetDatabase'"
    Invoke-Docker exec $ContainerName sh -lc "createdb -U `"`$POSTGRES_USER`" '$TargetDatabase'"

    Write-Host "Restoring archive into $TargetDatabase"
    Invoke-Docker exec $ContainerName sh -lc "pg_restore -U `"`$POSTGRES_USER`" -d '$TargetDatabase' --clean --if-exists --no-owner --no-acl '$remotePath'"

    Write-Host "Restore drill completed: $Environment/$TargetDatabase"
    Write-Host "The restore-check database is retained for verification; drop it after smoke checks."
}
finally {
    & docker exec $ContainerName rm -f $remotePath 2>$null | Out-Null
}
