# TestPublish.ps1 - 測試發佈流程的腳本
param(
    [string]$PublishProfile = "FolderProfile",
    [string]$Configuration = "Release"
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🧪 開始測試發佈流程" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 獲取當前目錄
$projectDir = Split-Path -Parent $PSScriptRoot
$projectFile = Join-Path $projectDir "NOBApp.csproj"
$publishDir = "C:\BOT\PS"

Write-Host "📁 項目目錄: $projectDir"
Write-Host "📁 發佈目錄: $publishDir"
Write-Host "⚙️  配置: $Configuration"
Write-Host "📋 發佈設定檔: $PublishProfile"
Write-Host ""

# 檢查項目文件
if (-not (Test-Path $projectFile)) {
    Write-Host "❌ 找不到項目文件: $projectFile" -ForegroundColor Red
    exit 1
}

# 檢查是否安裝了 dotnet
$dotnetCmd = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnetCmd) {
    Write-Host "❌ 找不到 dotnet 命令" -ForegroundColor Red
    exit 1
}

Write-Host "✅ dotnet 已找到"
Write-Host ""

# 執行 Publish 命令
Write-Host "🚀 開始執行 Publish..." -ForegroundColor Yellow
Write-Host ""

$publishCmd = @(
    "publish",
    $projectFile,
    "-c", $Configuration,
    "-p:PublishProfile=$PublishProfile"
)

Write-Host "執行命令: dotnet $publishCmd" -ForegroundColor Gray
Write-Host ""

& dotnet @publishCmd

if ($LASTEXITCODE -eq 0) {
  Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "✅ 發佈成功完成" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    
    # 驗證文件
    Write-Host "📋 驗證輸出文件："
    
    if (Test-Path $publishDir) {
$publishDirParent = Split-Path $publishDir
        
    # 檢查 ZIP 文件
        $zipFiles = Get-ChildItem $publishDirParent -Filter "v*.zip" -ErrorAction SilentlyContinue
        if ($zipFiles) {
 Write-Host "✅ ZIP 文件: $($zipFiles.Name)" -ForegroundColor Green
            $zipFiles | ForEach-Object {
             Write-Host "   大小: $([math]::Round($_.Length / 1MB, 2)) MB"
                Write-Host "   時間: $($_.LastWriteTime)"
     }
        } else {
            Write-Host "⚠️  未找到 ZIP 文件" -ForegroundColor Yellow
        }
        
        # 檢查版本信息
 $versionFile = Join-Path $projectDir "VersionInfo.cs"
        if (Test-Path $versionFile) {
            $content = Get-Content $versionFile -Raw
            if ($content -match 'public\s+const\s+string\s+Version\s*=\s*"([^"]+)"') {
                Write-Host "✅ 當前版本: $($matches[1])" -ForegroundColor Green
     }
      }
    } else {
        Write-Host "⚠️  發佈目錄不存在: $publishDir" -ForegroundColor Yellow
    }
} else {
    Write-Host ""
 Write-Host "========================================" -ForegroundColor Red
  Write-Host "❌ 發佈失敗" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "📝 提示："
Write-Host "  - 查看上面的輸出日誌，確認是否看到 '開始執行自訂發佈後置工作'"
Write-Host "  - 如果沒有看到，請檢查 .csproj 的 CustomActionsAfterPublish 目標"
Write-Host "  - 查看 GitHub Release: https://github.com/TwIcePenguin/nobapp/releases"
Write-Host ""
