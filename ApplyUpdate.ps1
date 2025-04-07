# ApplyUpdate.ps1
param(
    [string]$AppPath
)

# 清除終端並設置顏色
Clear-Host
$host.UI.RawUI.BackgroundColor = "Black"
$host.UI.RawUI.ForegroundColor = "White"
Clear-Host

# 定義彩色輸出函數
function Write-ColorText {
    param (
        [string]$Text,
        [string]$Color = "White"
    )
    
    Write-Host $Text -ForegroundColor $Color
}

# 定義日誌函數
function Log-Message {
    param (
        [string]$Message,
        [string]$Color = "Green"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "$timestamp - $Message"
    
    # 輸出到終端
    Write-ColorText $logMessage $Color
    
    # 寫入日誌檔
    $logPath = Join-Path $PSScriptRoot "update_log.txt"
    $logMessage | Out-File -Append -FilePath $logPath
}

# 顯示標題
Write-ColorText "`n=======================================================" "Cyan"
Write-ColorText "               企鵝之野望 - 更新程序                 " "Cyan"
Write-ColorText "=======================================================" "Cyan"
Write-ColorText "`n正在執行更新，請勿關閉此視窗...`n" "Yellow"

# 開始記錄
Log-Message "更新腳本開始執行"
Log-Message "應用程式路徑: $AppPath"
Log-Message "當前目錄: $PSScriptRoot"

# 檢查更新文件
$updateFilePath = Join-Path $PSScriptRoot "update.zip"
if (-not (Test-Path $updateFilePath)) {
    Log-Message "找不到更新檔案 $updateFilePath" "Red"
    Write-ColorText "`n更新失敗: 找不到更新檔案" "Red"
    Write-ColorText "`n按任意鍵關閉此視窗..." "Yellow"
    $null = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

# 顯示檔案大小
$fileSize = (Get-Item $updateFilePath).Length / 1MB
Log-Message "找到更新檔案，大小: $([Math]::Round($fileSize, 2)) MB"

# 等待一下以確保原應用程式退出
Write-ColorText "`n正在等待原應用程式退出..." "Yellow"
Start-Sleep -Seconds 2

# 解壓縮更新文件
try {
    Write-ColorText "`n開始解壓更新文件..." "Yellow"
    
    # 使用 .NET 內建解壓縮功能
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    
    # 先獲取 zip 檔內容資訊
    $zip = [System.IO.Compression.ZipFile]::OpenRead($updateFilePath)
    $totalFiles = $zip.Entries.Count
    Log-Message "更新包含 $totalFiles 個文件"
    
    # 列出要更新的檔案
    Write-ColorText "`n更新包含的主要檔案:" "Cyan"
    $zip.Entries | Where-Object { $_.Length -gt 1000 } | Select-Object -First 5 | ForEach-Object {
        Write-ColorText "  - $($_.FullName) ($('{0:N2}' -f ($_.Length / 1KB)) KB)" "Gray"
    }
    if ($zip.Entries.Count -gt 5) {
        Write-ColorText "  - ... 和其他 $($zip.Entries.Count - 5) 個檔案" "Gray"
    }
    $zip.Dispose()
    
    # 解壓縮
    Write-ColorText "`n正在解壓縮檔案，請稍候..." "Yellow"
    [System.IO.Compression.ZipFile]::ExtractToDirectory($updateFilePath, $PSScriptRoot, $true)
    
    Write-ColorText "解壓縮完成！" "Green"
    Log-Message "更新文件解壓成功"
} 
catch {
    Log-Message "解壓更新文件失敗: $_" "Red"
    Write-ColorText "`n更新失敗: 無法解壓縮檔案" "Red"
    Write-ColorText "錯誤詳情: $_" "Red"
    Write-ColorText "`n按任意鍵關閉此視窗..." "Yellow"
    $null = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

# 刪除更新文件
try {
    Write-ColorText "`n正在刪除更新檔案..." "Yellow"
    Remove-Item $updateFilePath -Force -ErrorAction Stop
    Write-ColorText "更新檔案已刪除" "Green"
    Log-Message "更新檔案已刪除"
}
catch {
    Write-ColorText "警告: 無法刪除更新檔案: $_" "Yellow"
    Log-Message "警告: 刪除更新文件失敗: $_" "Yellow"
    # 即使無法刪除更新文件，也繼續進行
}

# 確定應用程式路徑
if (-not $AppPath -or -not (Test-Path $AppPath)) {
    Write-ColorText "`n應用程式路徑無效，嘗試找到替代路徑..." "Yellow"
    Log-Message "應用程式路徑無效，嘗試找到替代路徑" "Yellow"
    
    $AppPath = Join-Path $PSScriptRoot "NOBApp.exe"
    
    if (-not (Test-Path $AppPath)) {
        Write-ColorText "找不到主應用程式執行檔" "Yellow"
        Log-Message "找不到主應用程式執行檔" "Yellow"
        
        # 嘗試查找任何 .exe 文件
        $exeFiles = Get-ChildItem -Path $PSScriptRoot -Filter "*.exe" -File
        if ($exeFiles.Count -gt 0) {
            $AppPath = $exeFiles[0].FullName
            Write-ColorText "找到可能的可執行文件: $AppPath" "Green"
            Log-Message "找到可能的可執行文件: $AppPath"
        }
        else {
            Write-ColorText "`n更新失敗: 找不到應用程式執行檔" "Red"
            Log-Message "找不到任何可執行文件，更新失敗" "Red"
            Write-ColorText "`n按任意鍵關閉此視窗..." "Yellow"
            $null = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
            exit 1
        }
    }
}

# 寫入更新成功的標記
$successMarker = Join-Path $PSScriptRoot "update_success.txt"
"更新成功於 $(Get-Date)" | Out-File -FilePath $successMarker
Log-Message "已寫入更新成功標記"

# 重新啟動程式
try {
    Write-ColorText "`n正在重新啟動應用程式..." "Yellow"
    Log-Message "準備重新啟動應用: $AppPath"
    Start-Process $AppPath
    Log-Message "應用程式已重新啟動"
    Write-ColorText "應用程式已重新啟動" "Green"
}
catch {
    Write-ColorText "`n無法重新啟動應用程式: $_" "Red"
    Log-Message "應用程式重新啟動失敗: $_" "Red"
    Write-ColorText "`n按任意鍵關閉此視窗..." "Yellow"
    $null = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

# 完成更新
Write-ColorText "`n=======================================================" "Cyan"
Write-ColorText "                  更新成功完成!                       " "Green"
Write-ColorText "=======================================================" "Cyan"
Log-Message "更新過程完成"

Write-ColorText "`n可以安全關閉此視窗，或按任意鍵關閉..." "Yellow"
$null = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
