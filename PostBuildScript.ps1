<#
PostBuildScript.ps1 - Create or update GitHub Release and upload ZIP asset
Parameters:
  -OutputPath: directory containing the ZIP file(s)
  -VersionInfoPath: path to VersionInfo.cs (used to extract Version constant)
  -GitHubToken: personal access token or provide via env GITHUB_TOKEN
  -GitHubRepo: owner/repo (default TwIcePenguin/nobapp in original)
  -GitFolder: git repository folder (default current)
  -DryRun: switch to print actions without performing network/git changes
#>
param (
    [string]$OutputPath = "",
    [string]$VersionInfoPath = "",
    [string]$GitHubToken = "",
    [string]$GitHubRepo = "TwIcePenguin/nobappspace",
    [string]$GitFolder = ".",
    [switch]$DryRun = $false
)

# Use environment token if not provided
if ([string]::IsNullOrEmpty($GitHubToken)) {
    $GitHubToken = $env:GITHUB_TOKEN
}

function Write-DebugLine { param($s) Write-Host $s -ForegroundColor Gray }

Write-Host ""; Write-Host "========================================" -ForegroundColor Cyan
Write-Host "GitHub Release helper" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($DryRun) {
    Write-Host "DRY RUN: no changes will be pushed or uploaded" -ForegroundColor Yellow
}

Write-DebugLine "DEBUG: OutputPath = '$OutputPath'"
Write-DebugLine "DEBUG: VersionInfoPath = '$VersionInfoPath'"
Write-DebugLine "DEBUG: GitHubToken = $(if ([string]::IsNullOrEmpty($GitHubToken)) { '(empty)' } else { '(set)' })"
Write-Host ""

if ($OutputPath -and -not $OutputPath.EndsWith('\')) { $OutputPath = $OutputPath + '\' }

if ([string]::IsNullOrEmpty($OutputPath)) {
    Write-Host "Error: OutputPath is required." -ForegroundColor Red
    exit 0
}

if ([string]::IsNullOrEmpty($VersionInfoPath)) {
    Write-Host "Error: VersionInfoPath is required." -ForegroundColor Red
    exit 0
}

if (-not (Test-Path $VersionInfoPath)) {
    Write-Host "Error: VersionInfoPath not found: $VersionInfoPath" -ForegroundColor Red
    exit 0
}

try {
    $content = Get-Content $VersionInfoPath -Raw -Encoding UTF8
} catch {
    $content = Get-Content $VersionInfoPath -Raw
}

$versionPattern = 'public\s+const\s+string\s+Version\s*=\s*"([^"]+)"'
if ($content -match $versionPattern) {
    $newVersion = $matches[1]
} else {
    Write-Host "Error: Could not extract Version from VersionInfo.cs" -ForegroundColor Red
    exit 0
}

$zipFileName = "v$newVersion.zip"
$zipFilePath = Join-Path $OutputPath $zipFileName

Write-Host "Preparing to upload: $zipFilePath"
Write-Host ""

# 增強的 ZIP 檔案檢查，包含延遲和偵錯日誌
Write-Host "Waiting for file system..." -ForegroundColor Gray
# Wait up to 15s for the file to appear (polling), to handle filesystem delays or async creation
$maxWaitMs = 15000
$interval = 500
$elapsed = 0
while (-not (Test-Path $zipFilePath) -and $elapsed -lt $maxWaitMs) {
    Start-Sleep -Milliseconds $interval
    $elapsed += $interval
}

Write-Host "Checking for ZIP file at '$zipFilePath'..." -ForegroundColor Gray
if (-not (Test-Path $zipFilePath)) {
    Write-Host "Error: ZIP file not found: $zipFilePath" -ForegroundColor Red
    Write-Host "--- Debug: Listing contents of '$OutputPath' (recursive) ---" -ForegroundColor Yellow
    try {
        Get-ChildItem -Path $OutputPath -Recurse -Force -ErrorAction Stop | ForEach-Object {
            Write-Host "Found: $($_.FullName)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "Could not list directory '$OutputPath': $($_.Exception.Message)" -ForegroundColor Yellow
    }
    Write-Host "--------------------------------------------" -ForegroundColor Yellow
    exit 1
}

$zipSize = [math]::Round((Get-Item $zipFilePath).Length / 1MB, 2)
Write-Host ("ZIP size: {0}" -f $zipSize)
Write-Host ""

if (-not $GitHubToken) {
    Write-Host "Warning: GITHUB_TOKEN not set. Set -GitHubToken or env:GITHUB_TOKEN to proceed." -ForegroundColor Yellow
    exit 0
}

# Check git availability
$gitPath = Get-Command git -ErrorAction SilentlyContinue
if (-not $gitPath) {
    Write-Host "Warning: git not found on PATH." -ForegroundColor Yellow
    exit 0
}

try {
    Push-Location $GitFolder

    # Step 1: Commit VersionInfo.cs
    Write-Host "Step 1: Commit VersionInfo.cs"
    if ($DryRun) {
        Write-Host "[DRY RUN] git add VersionInfo.cs" -ForegroundColor Cyan
        Write-Host "[DRY RUN] git commit -m 'chore: Release v$newVersion'" -ForegroundColor Cyan
    } else {
        git add VersionInfo.cs 2>&1 | Out-Null
        $commitOutput = git commit -m "chore: Release v$newVersion" 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Committed VersionInfo.cs"
        } else {
            Write-Host "No commit created (possibly nothing to commit)." -ForegroundColor Yellow
        }
    }

    # Step 2: Push
    Write-Host "Step 2: Push to origin"
    if ($DryRun) {
        Write-Host "[DRY RUN] git push origin main" -ForegroundColor Cyan
    } else {
        $pushOutput = git push origin main 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Pushed to origin/main"
        } else {
            Write-Host "Warning: git push failed." -ForegroundColor Yellow
        }
    }

    # Step 3: Create or update GitHub Release
    Write-Host "Step 3: Create or update GitHub Release"
    $builtDate = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    $releaseBody = "## Automated Release`n`n**Version**: v$newVersion`n**Built**: $builtDate`n`n[Download v$newVersion.zip](https://github.com/$GitHubRepo/releases/download/v$newVersion/$zipFileName)"

    $releaseData = @{ tag_name = "v$newVersion"; name = "Release v$newVersion"; body = $releaseBody; draft = $false; prerelease = $false } | ConvertTo-Json

    $headers = @{ Authorization = "token $GitHubToken"; Accept = "application/vnd.github.v3+json"; "X-GitHub-Api-Version" = "2022-11-28" }

    $getUrl = "https://api.github.com/repos/$GitHubRepo/releases/tags/v$newVersion"
    $existingRelease = $null
    try {
        $existingRelease = Invoke-RestMethod -Uri $getUrl -Method GET -Headers $headers -ErrorAction SilentlyContinue
    } catch {
        # ignore
    }

    if ($existingRelease) {
        Write-Host "Release exists. Updating release..."
        $updateUrl = "https://api.github.com/repos/$GitHubRepo/releases/$($existingRelease.id)"
        if ($DryRun) {
            Write-Host "[DRY RUN] PATCH $updateUrl" -ForegroundColor Cyan
            Write-Host "[DRY RUN] Body: $releaseData" -ForegroundColor Cyan
        } else {
            Invoke-RestMethod -Uri $updateUrl -Method PATCH -Headers $headers -Body $releaseData -ErrorAction Stop | Out-Null
        }
    } else {
        Write-Host "Creating new release..."
        if ($DryRun) {
            Write-Host "[DRY RUN] POST https://api.github.com/repos/$GitHubRepo/releases" -ForegroundColor Cyan
            Write-Host "[DRY RUN] Body: $releaseData" -ForegroundColor Cyan
        } else {
            $releaseResponse = Invoke-RestMethod -Uri "https://api.github.com/repos/$GitHubRepo/releases" -Method POST -Headers $headers -Body $releaseData -ErrorAction Stop
        }
    }

    # Step 4: Upload asset
    Write-Host "Step 4: Upload ZIP asset to release"
    $finalRelease = Invoke-RestMethod -Uri $getUrl -Method GET -Headers $headers -ErrorAction Stop

    if ($finalRelease.upload_url) {
        $uploadUrl = $finalRelease.upload_url -replace '\{.*\}', "?name=$zipFileName"
        $fileContent = [System.IO.File]::ReadAllBytes($zipFilePath)
        $fileSizeMB = [math]::Round($fileContent.Length / 1MB, 2)

        $uploadHeaders = @{ Authorization = "token $GitHubToken"; "Content-Type" = "application/zip"; "X-GitHub-Api-Version" = "2022-11-28" }

        $existingAsset = $finalRelease.assets | Where-Object { $_.name -eq $zipFileName } | Select-Object -First 1
        if ($existingAsset) {
            Write-Host "Existing asset found, deleting..."
            $deleteUrl = "https://api.github.com/repos/$GitHubRepo/releases/assets/$($existingAsset.id)"
            if ($DryRun) {
                Write-Host "[DRY RUN] DELETE $deleteUrl" -ForegroundColor Cyan
            } else {
                Invoke-RestMethod -Uri $deleteUrl -Method DELETE -Headers $headers -ErrorAction SilentlyContinue | Out-Null
                Start-Sleep -Milliseconds 500
            }
        }

        Write-Host ("Uploading {0} ({1} MB)..." -f $zipFileName, $fileSizeMB)
        if ($DryRun) {
            Write-Host "[DRY RUN] POST $uploadUrl" -ForegroundColor Cyan
        } else {
            Invoke-RestMethod -Uri $uploadUrl -Method POST -Headers $uploadHeaders -Body $fileContent -ErrorAction Stop | Out-Null
        }

        Write-Host "Upload step finished."
    } else {
        Write-Host "Warning: release upload_url not available." -ForegroundColor Yellow
    }

    Write-Host ""; Write-Host "========================================" -ForegroundColor Green
    Write-Host "Release complete" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Version: v$newVersion" -ForegroundColor Green
    Write-Host "Release URL: https://github.com/$GitHubRepo/releases/tag/v$newVersion" -ForegroundColor Green
    Write-Host "Download URL: https://github.com/$GitHubRepo/releases/download/v$newVersion/$zipFileName" -ForegroundColor Green
    Write-Host ""

    Pop-Location

} catch {
    Write-Host ""; Write-Host "========================================" -ForegroundColor Red
    Write-Host "Error during release process" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ("Exception: {0}" -f $_.Exception.Message) -ForegroundColor Red
    Write-Host ""
    try { Pop-Location } catch {}
    exit 1
}
