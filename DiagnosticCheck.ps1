# DiagnosticCheck.ps1 - 診斷環境配置
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🔍 開始診斷環境配置" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. 檢查 dotnet
Write-Host "1️⃣  檢查 .NET 環境..."
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($dotnet) {
Write-Host "   ✅ dotnet 已安裝: $($dotnet.Source)"
    $version = & dotnet --version
    Write-Host "   ✅ 版本: $version"
} else {
    Write-Host "   ❌ 找不到 dotnet" -ForegroundColor Red
}

# 2. 檢查 Git
Write-Host ""
Write-Host "2️⃣  檢查 Git..."
$git = Get-Command git -ErrorAction SilentlyContinue
if ($git) {
    Write-Host "   ✅ Git 已安裝: $($git.Source)"
  $gitVersion = & git --version
  Write-Host "   ✅ 版本: $gitVersion"
} else {
    Write-Host "   ❌ 找不到 Git" -ForegroundColor Red
}

# 3. 檢查 7-Zip
Write-Host ""
Write-Host "3️⃣  檢查 7-Zip..."
$sevenZip = "C:\Program Files\7-Zip\7z.exe"
if (Test-Path $sevenZip) {
    Write-Host "   ✅ 7-Zip 已安裝: $sevenZip"
} else {
    Write-Host "   ❌ 7-Zip 未找到或路徑不正確" -ForegroundColor Red
    Write-Host "   💡 預期路徑: $sevenZip" -ForegroundColor Yellow
}

# 4. 檢查 GitHub Token
Write-Host ""
Write-Host "4️⃣  檢查 GitHub Token..."
$token = $env:GITHUB_TOKEN
if ($token) {
    Write-Host "   ✅ GITHUB_TOKEN 已設定"
    Write-Host "   ℹ️  Token 前 10 個字符: $($token.Substring(0, 10))..."
} else {
    Write-Host "   ⚠️  GITHUB_TOKEN 未設定" -ForegroundColor Yellow
    Write-Host "   💡 設定方式: \`$env:GITHUB_TOKEN = 'your_token'\`"
}

# 5. 檢查項目文件
Write-Host ""
Write-Host "5️⃣  檢查項目文件..."
$projectDir = "H:\MemberSystem\nobappGitHub"
$projectFile = Join-Path $projectDir "NOBApp.csproj"
$versionFile = Join-Path $projectDir "VersionInfo.cs"
$updateScript = Join-Path $projectDir "UpdateVersion.ps1"
$postBuildScript = Join-Path $projectDir "PostBuildScript.ps1"
$publishProfile = Join-Path $projectDir "Properties\PublishProfiles\FolderProfile.pubxml"

$files = @(
  @{ Path = $projectFile; Name = "NOBApp.csproj" }
    @{ Path = $versionFile; Name = "VersionInfo.cs" }
@{ Path = $updateScript; Name = "UpdateVersion.ps1" }
    @{ Path = $postBuildScript; Name = "PostBuildScript.ps1" }
    @{ Path = $publishProfile; Name = "FolderProfile.pubxml" }
)

foreach ($file in $files) {
    if (Test-Path $file.Path) {
        Write-Host "   ✅ $($file.Name) 已找到"
    } else {
        Write-Host "   ❌ $($file.Name) 未找到" -ForegroundColor Red
    }
}

# 6. 檢查發佈目錄
Write-Host ""
Write-Host "6️⃣  檢查發佈目錄..."
$publishDir = "C:\BOT\PS"
if (Test-Path $publishDir) {
    Write-Host "   ✅ 發佈目錄存在: $publishDir"
    $items = Get-ChildItem $publishDir -ErrorAction SilentlyContinue | Measure-Object
    Write-Host "   ℹ️  項目數量: $($items.Count)"
} else {
    Write-Host "   ⚠️發佈目錄不存在: $publishDir" -ForegroundColor Yellow
}

# 7. 檢查 Git 配置
Write-Host ""
Write-Host "7️⃣  檢查 Git 配置..."
Push-Location $projectDir
$gitStatus = git status --porcelain 2>$null
$gitRemote = git remote -v 2>$null | Select-Object -First 1
Pop-Location

if ($gitStatus -or $gitRemote) {
    Write-Host "   ✅ Git 倉庫已初始化"
    if ($gitRemote) {
     Write-Host "   ✅ Remote 已設定: $gitRemote"
    }
} else {
    Write-Host "   ❌ Git 倉庫未初始化或 remote 未設定" -ForegroundColor Red
}

# 8. 檢查當前版本
Write-Host ""
Write-Host "8️⃣  檢查當前版本..."
if (Test-Path $versionFile) {
    $content = Get-Content $versionFile -Raw
if ($content -match 'public\s+const\s+string\s+Version\s*=\s*"([^"]+)"') {
        Write-Host "   ✅ 當前版本: $($matches[1])"
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ 診斷完成" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "💡 後續步驟："
Write-Host "  1. 確認所有檢查項都是 ✅ 狀態"
Write-Host "  2. 如果缺少 7-Zip，請從 https://www.7-zip.org 下載安裝"
Write-Host "  3. 如果缺少 GitHub Token，請獲取並設定"
Write-Host "  4. 運行 TestPublish.ps1 測試完整發佈流程"
Write-Host ""
