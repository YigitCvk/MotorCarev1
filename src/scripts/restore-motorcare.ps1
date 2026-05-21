[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("staging", "production")]
    [string]$Environment,

    [Parameter(Mandatory = $true)]
    [string]$BackupFile,

    [Parameter(Mandatory = $true)]
    [string]$AttachmentsArchive,

    [string]$TargetDatabase = "motorcare_restore_check",

    [string]$PostgresContainerName,

    [string]$ApiContainerName,

    [string]$AttachmentsRestorePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($PostgresContainerName)) {
    $PostgresContainerName = if ($Environment -eq "staging") { "motorcare-staging-postgres" } else { "motorcare-production-postgres" }
}

if ([string]::IsNullOrWhiteSpace($ApiContainerName)) {
    $ApiContainerName = if ($Environment -eq "staging") { "motorcare-staging-api" } else { "motorcare-production-api" }
}

$liveAttachmentsPath = if (${env:Storage__AttachmentsPath}) { ${env:Storage__AttachmentsPath} } else { "/var/lib/motorcare/attachments" }
if ([string]::IsNullOrWhiteSpace($AttachmentsRestorePath)) {
    $AttachmentsRestorePath = "$liveAttachmentsPath/restore-check/$TargetDatabase"
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

Assert-ContainerPath -Path $liveAttachmentsPath
Assert-ContainerPath -Path $AttachmentsRestorePath

if ($AttachmentsRestorePath -eq $liveAttachmentsPath -or $AttachmentsRestorePath -eq "/") {
    throw "Refusing to restore attachments into live root: $AttachmentsRestorePath"
}

if (-not (Test-Path -LiteralPath $BackupFile -PathType Leaf)) {
    throw "Database backup file does not exist: $BackupFile"
}

if (-not (Test-Path -LiteralPath $AttachmentsArchive -PathType Leaf)) {
    throw "Attachment archive does not exist: $AttachmentsArchive"
}

if ((Get-Item -LiteralPath $BackupFile).Length -le 0) {
    throw "Database backup file is empty: $BackupFile"
}

if ((Get-Item -LiteralPath $AttachmentsArchive).Length -le 0) {
    throw "Attachment archive is empty: $AttachmentsArchive"
}

$timestamp = (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssZ")
$remoteAttachmentsPath = "/tmp/motorcare-attachments-restore-$timestamp.tar.gz"

try {
    Write-Host "Copying attachment archive into $ApiContainerName for validation"
    Invoke-Docker cp $AttachmentsArchive "${ApiContainerName}:$remoteAttachmentsPath"
    $entries = & docker exec $ApiContainerName sh -lc "tar -tzf '$remoteAttachmentsPath'"
    if ($LASTEXITCODE -ne 0) {
        throw "Attachment archive validation failed: $AttachmentsArchive"
    }

    foreach ($entry in $entries) {
        if ($entry.StartsWith("/", [StringComparison]::Ordinal) -or $entry -match '(^|/)\.\.(/|$)') {
            throw "Attachment archive contains unsafe path: $entry"
        }
    }

    & "$PSScriptRoot\restore-postgres.ps1" `
        -Environment $Environment `
        -BackupFile $BackupFile `
        -TargetDatabase $TargetDatabase `
        -ContainerName $PostgresContainerName

    Write-Host "Extracting attachments into restore-check path: ${ApiContainerName}:$AttachmentsRestorePath"
    Invoke-Docker exec $ApiContainerName sh -lc "mkdir -p '$AttachmentsRestorePath' && cd '$AttachmentsRestorePath' && tar -xzf '$remoteAttachmentsPath'"

    Write-Host "Restore drill completed: $Environment/$TargetDatabase"
    Write-Host "Attachment restore-check path retained for validation: $AttachmentsRestorePath"
}
finally {
    & docker exec $ApiContainerName rm -f $remoteAttachmentsPath 2>$null | Out-Null
}
