@echo off
REM PrePublishChecks.bat - 發佈前的完整檢查

cd /d "%~dp0"

echo.
echo ========================================
echo ✅ 發佈前檢查清單
echo ========================================
echo.

echo 1️⃣  檢查 GitHub Token...
echo ========================================
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "CheckGitHubToken.ps1"

echo.
echo 2️⃣  檢查硬編碼 Token...
echo ========================================
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "CheckForTokens.ps1"

echo.
echo ========================================
echo ✅ 檢查完成！
echo ========================================
echo.
echo 下一步：Build > Publish NOBApp...
echo.
pause
