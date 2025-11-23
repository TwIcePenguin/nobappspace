# PublishAndUpload.ps1 - 完整的發佈和上傳流程
# 這是一個獨立腳本，可以從 Visual Studio 的 "工具" → "外部工具" 調用

param(
    [string]$ProjectDir = "H:\MemberSystem\nobappGitHub",
    [string]$Configuration = "Release",
    [string]$PublishProfile = "FolderProfile"
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🚀 NOBApp 自動發佈流程啟動" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 進入項目目錄
Push-Location $ProjectDir

try {
    # Step 1: 執行 dotnet publish
    Write-Host "📦 步驟 1: 執行 dotnet publish..." -ForegroundColor Yellow
    Write-Host ""
    
    $publishCmd = @(
        "publish",
     "NOBApp.csproj",
     "-c", $Configuration,
    "-p:PublishProfile=$PublishProfile"
    )
    
    & dotnet @publishCmd
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Publish 失敗" -ForegroundColor Red
   exit 1
    }
    
    Write-Host ""
    Write-Host "✅ Publish 完成" -ForegroundColor Green
    Write-Host ""
    
  # Step 2: 執行版本更新和 GitHub 上傳
    Write-Host "📤 步驟 2: 執行版本更新和 GitHub 上傳..." -ForegroundColor Yellow
    Write-Host ""
    
  # 讀取版本
    $content = Get-Content "VersionInfo.cs" -Raw -Encoding UTF8
    $versionPattern = 'public\s+const\s+string\s+Version\s*=\s*"([^"]+)"'
    $currentVersion = if ($content -match $versionPattern) { $matches[1] } else { "未知" }
    
    Write-Host "當前版本: $currentVersion"
    
    # 執行 UpdateVersion.ps1
    Write-Host "更新版本號..."
    & powershell -NoProfile -ExecutionPolicy Bypass -File "UpdateVersion.ps1" -VersionFile "VersionInfo.cs" -Force
    
    # 讀取新版本
    $content = Get-Content "VersionInfo.cs" -Raw -Encoding UTF8
    $newVersion = if ($content -match $versionPattern) { $matches[1] } else { "未知" }
    Write-Host "新版本: $newVersion" -ForegroundColor Green
    Write-Host ""
    
    # 執行 PostBuildScript
    Write-Host "上傳到 GitHub..."
    $zipOutputDir = "C:\BOT\"
    & powershell -NoProfile -ExecutionPolicy Bypass -File "PostBuildScript.ps1" `
        -OutputPath $zipOutputDir `
        -VersionInfoPath "VersionInfo.cs" `
  -GitHubToken $env:GITHUB_TOKEN `
        -GitHubRepo "TwIcePenguin/nobapp" `
        -GitFolder $ProjectDir
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "✅ 完整發佈流程成功完成！" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "✅ 版本: v$newVersion"
    Write-Host "✅ 發佈目錄: C:\BOT\PS"
    Write-Host "✅ ZIP 檔案: C:\BOT\v$newVersion.zip"
    Write-Host "✅ GitHub Release: https://github.com/TwIcePenguin/nobapp/releases/tag/v$newVersion"
    Write-Host ""

} catch {
    Write-Host "❌ 發生錯誤: $_" -ForegroundColor Red
exit 1
} finally {
    Pop-Location
}
