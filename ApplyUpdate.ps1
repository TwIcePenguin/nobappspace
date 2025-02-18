# ApplyUpdate.ps1
$updateFilePath = "update.zip"
$extractPath = "."

# 解壓縮更新文件
Expand-Archive -Path $updateFilePath -DestinationPath $extractPath -Force

# 刪除更新文件
Remove-Item $updateFilePath

# 重新啟動程式
Start-Process "NOBApp.exe"