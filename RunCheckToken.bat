@echo off
REM RunCheckToken.bat - 直接執行 GitHub Token 檢查工具

cd /d "%~dp0"

echo.
echo ========================================
echo 🔐 GitHub Token 檢查工具
echo ========================================
echo.

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "CheckGitHubToken.ps1"

echo.
echo ========================================
echo 按 Enter 鍵關閉...
echo ========================================
pause
