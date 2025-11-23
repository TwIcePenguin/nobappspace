# DebugPublishProcess.ps1 - 調試發佈流程
# 此腳本用於測試 ZIP 創建和參數傳遞

param (
    [string]$ProjectDir = "H:\MemberSystem\nobappGitHub"
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🔍 調試發佈流程" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 讀取版本
Write-Host "步驟 1: 讀取版本號"
$versionFile = Join-Path $ProjectDir "VersionInfo.cs"
Write-Host "版本檔案: $versionFile"

if (Test-Path $versionFile) {
    $content = Get-Content $versionFile -Raw
    $versionPattern = 'public\s+const\s+string\s+Version\s*=\s*"([^"]+)"'
    if ($content -match $versionPattern) {
        $version = $matches[1]
        Write-Host "✅ 版本號: $version" -ForegroundColor Green
    } else {
        Write-Host "❌ 無法提取版本號" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "❌ 版本檔案不存在: $versionFile" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "步驟 2: 測試 7-Zip 命令"

$sevenZipPath = "C:\Program Files\7-Zip\7z.exe"
if (Test-Path $sevenZipPath) {
    Write-Host "✅ 7-Zip 已找到: $sevenZipPath" -ForegroundColor Green
    
    # 測試 7z.exe 是否可執行
    $version = & "$sevenZipPath" 2>&1 | Select-Object -First 1
    Write-Host "7-Zip 版本資訊: $version"
} else {
    Write-Host "❌ 7-Zip 未找到: $sevenZipPath" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "步驟 3: 檢查發佈目錄"

$publishDir = Join-Path $ProjectDir "bin\Release\net8.0-windows7.0\publish\win-x86\"
Write-Host "發佈目錄: $publishDir"

if (Test-Path $publishDir) {
    Write-Host "✅ 發佈目錄存在" -ForegroundColor Green
    $fileCount = (Get-ChildItem $publishDir -Recurse -File).Count
    Write-Host "檔案數量: $fileCount"
    
    # 列出前 10 個檔案
    Write-Host "前 10 個檔案:"
    Get-ChildItem $publishDir -File | Select-Object -First 10 | ForEach-Object {
        Write-Host "  - $($_.Name)"
    }
} else {
    Write-Host "⚠️  發佈目錄不存在: $publishDir" -ForegroundColor Yellow
    Write-Host "請先執行發佈: dotnet publish -c Release"
}

Write-Host ""
Write-Host "步驟 4: 測試 PostBuildScript 參數"

$outputPath = Join-Path $ProjectDir "bin\Release\net8.0-windows7.0\publish\win-x86\"
$versionInfoPath = Join-Path $ProjectDir "VersionInfo.cs"

Write-Host "OutputPath: $outputPath"
Write-Host "VersionInfoPath: $versionInfoPath"
Write-Host "OutputPath 是否存在: $(Test-Path $outputPath)"
Write-Host "VersionInfoPath 是否存在: $(Test-Path $versionInfoPath)"

Write-Host ""
Write-Host "步驟 5: 模擬 PostBuildScript 調用"
Write-Host ""

$scriptPath = Join-Path $ProjectDir "PostBuildScript.ps1"
Write-Host "將執行以下命令:"
Write-Host "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$scriptPath`" -OutputPath `"$outputPath`" -VersionInfoPath `"$versionInfoPath`" -GitHubToken `"test`" -GitHubRepo `"TwIcePenguin/nobapp`" -GitFolder `"$ProjectDir`""

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🔍 調試完成" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
