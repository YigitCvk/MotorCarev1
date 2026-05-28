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

function Get-ContainerImage {
    param([Parameter(Mandatory = $true)][string]$ContainerName)

    $image = (& docker inspect -f "{{.Image}}" $ContainerName).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($image)) {
        throw "Could not inspect image for container: $ContainerName"
    }

    return $image
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

$attachmentsArchiveItem = Get-Item -LiteralPath $AttachmentsArchive
$attachmentsArchiveDir = (Resolve-Path -LiteralPath $attachmentsArchiveItem.DirectoryName).Path
$attachmentsArchiveName = $attachmentsArchiveItem.Name
if ($attachmentsArchiveName -notmatch '^[A-Za-z0-9_.-]+$') {
    throw "Attachment archive file name contains unsupported characters: $attachmentsArchiveName"
}

Write-Host "Validating attachment archive with $ApiContainerName volumes"
$apiImage = Get-ContainerImage -ContainerName $ApiContainerName
$entries = & docker run --rm --user 0:0 --volumes-from $ApiContainerName -v "${attachmentsArchiveDir}:/restore:ro" --entrypoint sh $apiImage -lc "tar -tzf '/restore/$attachmentsArchiveName'"
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
Invoke-Docker run --rm --user 0:0 --volumes-from $ApiContainerName -v "${attachmentsArchiveDir}:/restore:ro" --entrypoint sh $apiImage -lc "mkdir -p '$AttachmentsRestorePath' && cd '$AttachmentsRestorePath' && tar -xzf '/restore/$attachmentsArchiveName'"

Write-Host "Restore drill completed: $Environment/$TargetDatabase"
Write-Host "Attachment restore-check path retained for validation: $AttachmentsRestorePath"
