# PostBuildScript.ps1 - GitHub Release 上傳腳本
param (
    [string]$OutputPath = "",
    [string]$VersionInfoPath = "",
    [string]$GitHubToken = "",
    [string]$GitHubRepo = "TwIcePenguin/nobappspace",
    [string]$GitFolder = "."
)

# 如果參數為空，嘗試從環境變量獲取
if ([string]::IsNullOrEmpty($GitHubToken)) {
    $GitHubToken = $env:GITHUB_TOKEN
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "📤 GitHub Release 上傳腳本啟動" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 調試參數
Write-Host "DEBUG: OutputPath = '$OutputPath'" -ForegroundColor Gray
Write-Host "DEBUG: VersionInfoPath = '$VersionInfoPath'" -ForegroundColor Gray
Write-Host "DEBUG: GitHubToken = $(if ([string]::IsNullOrEmpty($GitHubToken)) { '(empty)' } else { '(set)' })" -ForegroundColor Gray
Write-Host ""

# 確保輸出路徑以反斜杠結尾
if ($OutputPath -and -not $OutputPath.EndsWith("\")) {
    $OutputPath = $OutputPath + "\"
}

Write-Host "📁 輸出路徑: $OutputPath"

# 驗證參數
if ([string]::IsNullOrEmpty($OutputPath)) {
    Write-Host "❌ 未提供輸出路徑 (OutputPath)" -ForegroundColor Red
    exit 0
}

if ([string]::IsNullOrEmpty($VersionInfoPath)) {
    Write-Host "❌ 未提供版本檔案路徑 (VersionInfoPath)" -ForegroundColor Red
  exit 0
}

# 讀取版本號
if (-not (Test-Path $VersionInfoPath)) {
    Write-Host "❌ 找不到版本文件：$VersionInfoPath" -ForegroundColor Red
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
    Write-Host "❌ 無法從版本文件中提取版本號" -ForegroundColor Red
    exit 0
}

$zipFileName = "v$newVersion.zip"
$zipFilePath = Join-Path $OutputPath $zipFileName

Write-Host "📦 版本：$newVersion"
Write-Host "📁 ZIP 檔案：$zipFilePath"
Write-Host ""

# 檢查 ZIP 檔案是否存在
if (-not (Test-Path $zipFilePath)) {
    Write-Host "⚠️  ZIP 檔案不存在，跳過上傳" -ForegroundColor Yellow
    Write-Host "📁 期望位置: $zipFilePath"
    Write-Host "📋 檢查目錄內容:"
    Get-ChildItem $OutputPath -Filter "*.zip" -ErrorAction SilentlyContinue | ForEach-Object {
  $sizeMB = [math]::Round($_.Length / 1MB, 2)
        Write-Host "   - $($_.Name) ($sizeMB MB)"
    }
    exit 0
}

Write-Host "✅ ZIP 檔案已找到"
$zipSize = [math]::Round((Get-Item $zipFilePath).Length / 1MB, 2)
Write-Host "📊 大小: $zipSize MB"
Write-Host ""

# 檢查是否設定了 GitHub Token
if (-not $GitHubToken) {
    Write-Host "⚠️  未設定 GITHUB_TOKEN，跳過 GitHub 上傳" -ForegroundColor Yellow
    Write-Host "📝 設定方式: `$env:GITHUB_TOKEN = 'your_token_here'" -ForegroundColor Yellow
    exit 0
}

Write-Host "✅ GITHUB_TOKEN 已設定"
Write-Host ""

# 檢查 Git 是否可用
$gitPath = Get-Command git -ErrorAction SilentlyContinue
if (-not $gitPath) {
    Write-Host "⚠️  找不到 Git，跳過 GitHub 上傳" -ForegroundColor Yellow
    exit 0
}

Write-Host "✅ Git 已找到"
Write-Host ""

try {
    Push-Location $GitFolder
  
    # 步驟 1: 提交版本更新
    Write-Host "📤 步驟 1: 提交版本更新到 Git..."
    git add VersionInfo.cs 2>&1 | Out-Null
  
    $commitOutput = git commit -m "chore: Release v$newVersion" 2>&1
    if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ 版本已提交"

        # 推送到 GitHub
        Write-Host "📤 步驟 2: 推送到 GitHub..."
  $pushOutput = git push origin main 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ 已推送到 GitHub"
        } else {
        Write-Host "⚠️  推送到 GitHub 失敗或已同步（可能是因為沒有新提交）"
        }
    } else {
        Write-Host "⚠️  沒有需要提交的更改（版本未改變）" -ForegroundColor Yellow
    }
    
    # 步驟 2: 檢查並建立 GitHub Release
    Write-Host ""
    Write-Host "📤 步驟 3: 建立/更新 GitHub Release..."
    
    $builtDate = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    $releaseData = @{
        tag_name   = "v$newVersion"
        name      = "Release v$newVersion"
      body         = "## Automated Release`n`n**Version**: v$newVersion`n**Built**: $builtDate`n`n[Download v$newVersion.zip](https://github.com/$GitHubRepo/releases/download/v$newVersion/$zipFileName)"
      draft        = $false
    prerelease = $false
    } | ConvertTo-Json
    
    $headers = @{
    "Authorization"    = "token $GitHubToken"
        "Accept"                = "application/vnd.github.v3+json"
        "X-GitHub-Api-Version"  = "2022-11-28"
    }
    
    # 先嘗試獲取現有的 Release
    $getUrl = "https://api.github.com/repos/$GitHubRepo/releases/tags/v$newVersion"
    $existingRelease = $null
    
    try {
      $existingRelease = Invoke-RestMethod -Uri $getUrl -Method GET -Headers $headers -ErrorAction SilentlyContinue
    } catch {
        # Release 不存在，這是正常的
    }
    
    if ($existingRelease) {
        Write-Host "ℹ️  Release v$newVersion 已存在，將更新 Release"
     $updateUrl = "https://api.github.com/repos/$GitHubRepo/releases/$($existingRelease.id)"
        Invoke-RestMethod -Uri $updateUrl -Method PATCH -Headers $headers -Body $releaseData -ErrorAction Stop | Out-Null
    } else {
     Write-Host "ℹ️  建立新的 Release v$newVersion"
     $releaseResponse = Invoke-RestMethod -Uri "https://api.github.com/repos/$GitHubRepo/releases" -Method POST -Headers $headers -Body $releaseData -ErrorAction Stop
 }
    
    # 步驟 3: 上傳 ZIP 檔案
    Write-Host ""
    Write-Host "📤 步驟 4: 上傳 ZIP 檔案到 Release..."
    
    # 再次獲取 Release 資訊以獲得上傳 URL
    $finalRelease = Invoke-RestMethod -Uri $getUrl -Method GET -Headers $headers -ErrorAction Stop
    
    if ($finalRelease.upload_url) {
        $uploadUrl = $finalRelease.upload_url -replace '\{.*\}', "?name=$zipFileName"
      
    $fileContent = [System.IO.File]::ReadAllBytes($zipFilePath)
        $fileSizeMB = [math]::Round($fileContent.Length / 1MB, 2)
        
        $uploadHeaders = @{
    "Authorization"     = "token $GitHubToken"
      "Content-Type"          = "application/zip"
    "X-GitHub-Api-Version"  = "2022-11-28"
     }
        
        # 檢查該文件是否已存在於 Release，如果存在則先刪除
        $existingAsset = $finalRelease.assets | Where-Object { $_.name -eq $zipFileName } | Select-Object -First 1
 
        if ($existingAsset) {
            Write-Host "🗑️  刪除舊的資源文件..."
  $deleteUrl = "https://api.github.com/repos/$GitHubRepo/releases/assets/$($existingAsset.id)"
            Invoke-RestMethod -Uri $deleteUrl -Method DELETE -Headers $headers -ErrorAction SilentlyContinue | Out-Null
          Start-Sleep -Milliseconds 500  # GitHub API 可能需要時間處理刪除
        }
        
        # 上傳新文件
        Write-Host "⬆️  上傳 $zipFileName ($fileSizeMB MB)..."
   $uploadResponse = Invoke-RestMethod -Uri $uploadUrl -Method POST -Headers $uploadHeaders -Body $fileContent -ErrorAction Stop
    
        Write-Host "✅ ZIP 檔案已上傳"
} else {
        Write-Host "⚠️  Release 沒有 upload_url，無法上傳" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "✅ 上傳完成" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "✅ 版本: v$newVersion" -ForegroundColor Green
Write-Host "✅ Release URL: https://github.com/$GitHubRepo/releases/tag/v$newVersion"
    Write-Host "✅ Download URL: https://github.com/$GitHubRepo/releases/download/v$newVersion/$zipFileName"
    Write-Host ""
    
    Pop-Location
    
} catch {
    Write-Host ""
  Write-Host "========================================" -ForegroundColor Red
    Write-Host "❌ 上傳到 GitHub 時發生錯誤" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "❌ 錯誤詳情: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Pop-Location
    exit 1
}
