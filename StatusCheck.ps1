# StatusCheck.ps1 - 發佈系統狀態檢查

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🔍 NOBApp 發佈系統狀態檢查" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$allGood = $true

# 檢查 1：Git 配置
Write-Host "1️⃣  檢查 Git 配置..." -ForegroundColor Yellow
$gitName = git config --global user.name
$gitEmail = git config --global user.email

if ([string]::IsNullOrEmpty($gitName) -or [string]::IsNullOrEmpty($gitEmail)) {
    Write-Host "   ❌ Git 用戶信息未配置" -ForegroundColor Red
    Write-Host "   📝 設置方式：" -ForegroundColor Yellow
    Write-Host "      git config --global user.name 'Your Name'" -ForegroundColor Gray
Write-Host "      git config --global user.email 'your@email.com'" -ForegroundColor Gray
    $allGood = $false
} else {
    Write-Host "   ✅ Git 用戶：$gitName <$gitEmail>" -ForegroundColor Green
}

# 檢查 2：GitHub Token
Write-Host ""
Write-Host "2️⃣  檢查 GitHub Token..." -ForegroundColor Yellow
$token = $env:GITHUB_TOKEN
if ([string]::IsNullOrEmpty($token)) {
    Write-Host "   ⚠️  GITHUB_TOKEN 未設置" -ForegroundColor Yellow
    Write-Host "   📝 設置方式：`$env:GITHUB_TOKEN = 'your_token'" -ForegroundColor Gray
} else {
    Write-Host "   ✅ GITHUB_TOKEN 已設置" -ForegroundColor Green
}

# 檢查 3：版本號文件
Write-Host ""
Write-Host "3️⃣  檢查版本號文件..." -ForegroundColor Yellow
if (Test-Path "VersionInfo.cs") {
    $versionContent = Get-Content "VersionInfo.cs" -Raw
    if ($versionContent -match 'public\s+const\s+string\s+Version\s*=\s*"([^"]+)"') {
    $version = $matches[1]
      Write-Host "   ✅ 當前版本：$version" -ForegroundColor Green
 } else {
  Write-Host "   ❌ 版本號格式錯誤" -ForegroundColor Red
        $allGood = $false
    }
} else {
    Write-Host "   ❌ VersionInfo.cs 不存在" -ForegroundColor Red
    $allGood = $false
}

# 檢查 4：發佈配置
Write-Host ""
Write-Host "4️⃣  檢查發佈配置..." -ForegroundColor Yellow
$pubProfile = "Properties\PublishProfiles\FolderProfile.pubxml"
if (Test-Path $pubProfile) {
    Write-Host "   ✅ FolderProfile 發佈配置已存在" -ForegroundColor Green
} else {
    Write-Host "   ❌ FolderProfile 發佈配置不存在" -ForegroundColor Red
    $allGood = $false
}

# 檢查 5：PowerShell 腳本
Write-Host ""
Write-Host "5️⃣  檢查 PowerShell 腳本..." -ForegroundColor Yellow
$scripts = @("UpdateVersion.ps1", "CreateZip.ps1", "PostBuildScript.ps1")
foreach ($script in $scripts) {
    if (Test-Path $script) {
        Write-Host "   ✅ $script" -ForegroundColor Green
    } else {
      Write-Host "   ❌ $script 不存在" -ForegroundColor Red
        $allGood = $false
    }
}

# 檢查 6：項目文件
Write-Host ""
Write-Host "6️⃣  檢查項目文件..." -ForegroundColor Yellow
if (Test-Path "NOBApp.csproj") {
    Write-Host "   ✅ NOBApp.csproj 存在" -ForegroundColor Green
    
    $csproj = Get-Content "NOBApp.csproj" -Raw
    if ($csproj -contains "CustomActionsAfterPublish") {
        Write-Host "   ✅ 發佈後置任務已配置" -ForegroundColor Green
    }
} else {
    Write-Host "   ❌ NOBApp.csproj 不存在" -ForegroundColor Red
    $allGood = $false
}

# 檢查 7：最近的 Release
Write-Host ""
Write-Host "7️⃣  檢查最近的 GitHub Release..." -ForegroundColor Yellow
$lastRelease = git describe --tags --abbrev=0 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ 最近的 Release：$lastRelease" -ForegroundColor Green
} else {
    Write-Host "   ⚠️  尚未發佈任何 Release" -ForegroundColor Yellow
}

# 檢查 8：磁盤空間
Write-Host ""
Write-Host "8️⃣  檢查磁盤空間..." -ForegroundColor Yellow
$drive = (Get-Location).Drive.Name
$driveInfo = Get-Volume -DriveLetter $drive
$freeSpace = [math]::Round($driveInfo.SizeRemaining / 1GB, 2)
if ($freeSpace -gt 5) {
    Write-Host "   ✅ 可用空間：$freeSpace GB（充足）" -ForegroundColor Green
} else {
    Write-Host "   ⚠️  可用空間：$freeSpace GB（可能不足）" -ForegroundColor Yellow
}

# 最終結果
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
if ($allGood) {
    Write-Host "✅ 系統狀態：就緒" -ForegroundColor Green
    Write-Host "🚀 可以立即發佈" -ForegroundColor Green
} else {
    Write-Host "⚠️  系統狀態：需要配置" -ForegroundColor Yellow
    Write-Host "📋 請解決上述問題後重試" -ForegroundColor Yellow
}
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "💡 提示：使用 'Build > Publish NOBApp...' 開始發佈" -ForegroundColor Cyan
Write-Host ""
