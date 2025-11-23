@echo off
REM RunCheckForTokens.bat - 檢查代碼中的硬編碼 Token

cd /d "%~dp0"

echo.
echo ========================================
echo 🔍 檢查硬編碼 Token
echo ========================================
echo.

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "CheckForTokens.ps1"

echo.
echo ========================================
echo 按 Enter 鍵關閉...
echo ========================================
pause
