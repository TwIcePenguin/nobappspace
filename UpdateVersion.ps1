param(
    [string]$VersionFile = "VersionInfo.cs",
    [string]$Configuration = $env:CONFIGURATION,
    [switch]$Force
)

# 如果環境變數名稱不同，嘗試其他常見變數
if (-not $Configuration) { $Configuration = $env:Configuration }
if (-not $Configuration) { $Configuration = "Debug" }

$versionPattern = 'public\s+const\s+string\s+Version\s*=\s*"(\d+)\.(\d+)\.(\d+)"'

if (-not (Test-Path $VersionFile)) {
    Write-Host "找不到檔案：$VersionFile"
    exit 0
}

# 以 UTF8 讀取，若失敗則不指定編碼再嘗試一次
try {
    $content = Get-Content -Path $VersionFile -Raw -Encoding UTF8
} catch {
    $content = Get-Content -Path $VersionFile -Raw
}

if ($content -match $versionPattern) {
    $majorVersion = $matches[1]
    $minorVersion = $matches[2]
    $buildNumber = [int]$matches[3]
  
    if ($Configuration -ieq 'Release' -or $Force.IsPresent) {
        $buildNumber++
        $newVersion = "$majorVersion.$minorVersion.$buildNumber"
        $newContent = $content -replace $versionPattern, "public const string Version = `"$newVersion`""
        
        try {
            # 以 UTF8 寫回（常見 CI 與編輯器都能正確處理 UTF8）
            Set-Content -Path $VersionFile -Value $newContent -Encoding UTF8 -ErrorAction Stop
            Write-Host "已更新版本為 $newVersion （組態: $Configuration）"
        } catch {
            Write-Host "❌ 寫入版本檔案 '$VersionFile' 失敗！" -ForegroundColor Red
            Write-Host "錯誤詳情: $($_.Exception.Message)" -ForegroundColor Red
            exit 1
        }
        # 輸出版本號供調用端使用
        Write-Output $newVersion
    } else {
        Write-Host "目前組態為 '$Configuration'，僅在 'Release' 或使用 -Force 時才會更新版本號。當前版本：$majorVersion.$minorVersion.$buildNumber"
        Write-Output "$majorVersion.$minorVersion.$buildNumber"
    }
} else {
    Write-Host "在 $VersionFile 中找不到符合的版本字串（格式應為 public const string Version = \"X.Y.Z\"）。"
    exit 1
}
