# CreateZip.ps1 - Create a ZIP file from a publish win-x86 folder
param (
    [string]$SourcePath = "",
    [string]$ZipPath = ""
)

$ErrorActionPreference = 'Stop'

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Create ZIP from publish folder (expected: win-x86)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Trim and normalize inputs
if ($SourcePath) { $SourcePath = $SourcePath.Trim() }
if ($ZipPath) { $ZipPath = $ZipPath.Trim() }

if ([string]::IsNullOrEmpty($SourcePath)) { Write-Host "Error: SourcePath is required." -ForegroundColor Red; exit 1 }
if ([string]::IsNullOrEmpty($ZipPath)) { Write-Host "Error: ZipPath is required." -ForegroundColor Red; exit 1 }

# Resolve SourcePath to absolute path when possible
try {
    $resolvedSource = Resolve-Path -Path $SourcePath -ErrorAction Stop
    $SourcePath = $resolvedSource.Path
} catch {
    # If Resolve-Path fails, try to combine with current directory
    $combined = Join-Path -Path (Get-Location).Path -ChildPath $SourcePath
    if (Test-Path $combined) { $SourcePath = (Resolve-Path $combined).Path } else { Write-Host "Error: SourcePath not found: $SourcePath" -ForegroundColor Red; exit 1 }
}

# Ensure ZipPath is a full path
try {
    $zipFull = [System.IO.Path]::GetFullPath($ZipPath)
    $ZipPath = $zipFull
} catch {
    Write-Host "Error: Invalid ZipPath: $ZipPath" -ForegroundColor Red
    exit 1
}

# Verify SourcePath ends with win-x86 (case-insensitive)
$leaf = Split-Path $SourcePath -Leaf
if ($leaf.ToLowerInvariant() -ne 'win-x86') {
    Write-Host "Warning: Expected SourcePath leaf 'win-x86' but found '$leaf'" -ForegroundColor Yellow
    # Continue anyway
}

# Ensure target directory exists
$targetDir = [System.IO.Path]::GetDirectoryName($ZipPath)
if (-not (Test-Path $targetDir)) {
    try { New-Item -ItemType Directory -Path $targetDir -Force | Out-Null } catch { Write-Host "Error: Cannot create target directory: $targetDir ($($_.Exception.Message))" -ForegroundColor Red; exit 1 }
}

Write-Host ("Source: {0}" -f $SourcePath) -ForegroundColor Gray
Write-Host ("Zip:    {0}" -f $ZipPath) -ForegroundColor Gray

# Collect files
$allFiles = Get-ChildItem -Path $SourcePath -Recurse -File -ErrorAction SilentlyContinue
if (-not $allFiles -or $allFiles.Count -eq 0) { Write-Host "Warning: No files found under SourcePath: $SourcePath" -ForegroundColor Yellow; exit 1 }

# Create staging area
$stagingDir = Join-Path ([System.IO.Path]::GetTempPath()) ("nobapp_staging_" + [Guid]::NewGuid().ToString())
try { New-Item -ItemType Directory -Path $stagingDir | Out-Null } catch { Write-Host "Error: Cannot create staging dir: $($stagingDir)" -ForegroundColor Red; exit 1 }
Write-Host ("Staging: {0}" -f $stagingDir) -ForegroundColor Gray

# Copy files into staging preserving relative paths
foreach ($f in $allFiles) {
    $rel = $f.FullName.Substring($SourcePath.Length).TrimStart('\','/')
    $destFile = Join-Path $stagingDir $rel
    $destDir = [System.IO.Path]::GetDirectoryName($destFile)
    if (-not (Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir | Out-Null }
    try { Copy-Item -Path $f.FullName -Destination $destFile -Force -ErrorAction Stop } catch { Write-Host ("Warning: copy failed: {0} -> {1} : {2}" -f $f.FullName, $destFile, $_.Exception.Message) -ForegroundColor Yellow }
}

# Ensure Zip APIs are available
try { Add-Type -AssemblyName System.IO.Compression.FileSystem -ErrorAction Stop } catch { Write-Host ("Error: Could not load compression assembly: {0}" -f $_.Exception.Message) -ForegroundColor Red; Remove-Item $stagingDir -Recurse -Force; exit 1 }

# Remove existing zip if any
if (Test-Path $ZipPath) {
    try { Remove-Item $ZipPath -Force -ErrorAction Stop } catch { Write-Host ("Warning: cannot remove existing zip: {0}" -f $_.Exception.Message) -ForegroundColor Yellow }
}

# Create zip
try {
    Write-Host "Creating ZIP from staging..." -ForegroundColor Cyan
    [System.IO.Compression.ZipFile]::CreateFromDirectory($stagingDir, $ZipPath, [System.IO.Compression.CompressionLevel]::Optimal, $false)
    Write-Host "ZIP created." -ForegroundColor Green
} catch {
    Write-Host ("Error: Failed to create ZIP: {0}" -f $_.Exception.Message) -ForegroundColor Red
    try { if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force } } catch {}
    Remove-Item $stagingDir -Recurse -Force
    exit 1
}

# Cleanup staging
try { Remove-Item $stagingDir -Recurse -Force -ErrorAction SilentlyContinue } catch { }

# Verify zip exists
if (Test-Path $ZipPath) {
    $zipSizeMB = [math]::Round((Get-Item $ZipPath).Length / 1MB, 2)
    Write-Host ("ZIP size: {0} MB" -f $zipSizeMB) -ForegroundColor Green
    Write-Host ("ZIP path: {0}" -f $ZipPath) -ForegroundColor Green
    exit 0
} else {
    Write-Host "Error: ZIP file not found after creation." -ForegroundColor Red
    exit 1
}
