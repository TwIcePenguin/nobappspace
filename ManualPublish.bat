@echo off
REM 完整發佈流程 - 支持 Visual Studio 集成

setlocal enabledelayedexpansion

set PROJECT_DIR=H:\MemberSystem\nobappGitHub
set CONFIG=Release
set PUBLISH_PROFILE=FolderProfile

echo.
echo ========================================
echo 🚀 開始自動發佈流程
echo ========================================
echo.

REM 檢查是否在項目目錄
cd /d "%PROJECT_DIR%"

REM Step 1: 執行 dotnet publish
echo 📦 步驟 1: 執行 Publish...
echo.

dotnet publish NOBApp.csproj -c %CONFIG% -p:PublishProfile=%PUBLISH_PROFILE% -v quiet

if errorlevel 1 (
    echo.
    echo ❌ Publish 失敗
    echo.
    pause
    exit /b 1
)

echo ✅ Publish 完成
echo.

REM Step 2: 執行 PowerShell 腳本進行版本更新和 GitHub 上傳
echo 📤 步驟 2: 版本更新和 GitHub 上傳...
echo.

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
 "Set-Location '%PROJECT_DIR%'; ^
    & '.\PublishAndUpload.ps1' -ProjectDir '%PROJECT_DIR%' -Configuration '%CONFIG%' -PublishProfile '%PUBLISH_PROFILE%'""

if errorlevel 1 (
    echo.
    echo ⚠️ 腳本執行失敗，但 Publish 已完成
    echo.
 pause
    exit /b 0
)

echo.
echo ========================================
echo ✅ 發佈流程完成
echo ========================================
echo.
