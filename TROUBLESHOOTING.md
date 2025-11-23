# 🆘 故障排除指南

## 問題 1: CustomActionsAfterPublish 沒有執行

### 症狀
- 發佈完成，但沒有看到 "🚀 開始執行自訂發佈後置工作" 消息
- VersionInfo.cs 版本號未更新
- C:\BOT\ 中沒有新的 ZIP 文件

### 原因分析

| 原因 | 徵兆 | 解決方案 |
|------|------|--------|
| 構建配置不是 Release | 發佈時選擇了 Debug 配置 | 確保選擇 Release 配置 |
| PublishDir 為空 | 發佈失敗 | 檢查編譯是否成功 |
| .csproj 文件無效 | 構建時有錯誤 | 執行 `dotnet build` 驗證 |
| PowerShell 執行策略受限 | 看不到任何 PS 輸出 | 使用 `-ExecutionPolicy Bypass` |

### 診斷步驟

```powershell
# 1. 檢查 .csproj 是否有正確的目標定義
Get-Content "H:\MemberSystem\nobappGitHub\NOBApp.csproj" | Select-String "CustomActionsAfterPublish"
# 應該返回: <Target Name="CustomActionsAfterPublish" AfterTargets="Publish"

# 2. 驗證項目構建成功
cd "H:\MemberSystem\nobappGitHub"
dotnet build -c Release
# 應該看到: "構建成功"

# 3. 增加 MSBuild 日誌詳細程度
dotnet publish NOBApp.csproj -c Release -p:PublishProfile=FolderProfile -v diag > publish.log 2>&1
# 查看 publish.log 中是否有 CustomActionsAfterPublish 執行記錄

# 4. 檢查 PublishDir 是否存在且不為空
Get-ChildItem "C:\BOT\PS" | Measure-Object
# 應該返回數個文件
```

---

## 問題 2: 版本號未自動更新

### 症狀
- 執行發佈後，VersionInfo.cs 中版本號未改變
- 沒有看到 "✅ 版本號已更新" 的消息

### 原因分析

| 原因 | 診斷命令 | 解決方案 |
|------|--------|--------|
| VersionInfo.cs 不存在 | `Test-Path "VersionInfo.cs"` | 創建文件，查看 QUICK_START.md |
| 版本格式不正確 | `Get-Content VersionInfo.cs` | 版本必須是 X.Y.Z 格式 |
| UpdateVersion.ps1 未執行 | 查看發佈日誌 | 檢查 PowerShell 執行權限 |
| 權限不足 | 嘗試手動編輯文件 | 以管理員身份運行 |

### 診斷步驟

```powershell
# 1. 檢查 VersionInfo.cs 的內容和格式
Get-Content "H:\MemberSystem\nobappGitHub\VersionInfo.cs" | Select-String "Version"
# 應該看到: public const string Version = "0.84.X";

# 2. 手動測試 UpdateVersion.ps1
cd "H:\MemberSystem\nobappGitHub"
powershell -ExecutionPolicy Bypass -File "UpdateVersion.ps1" -VersionFile "VersionInfo.cs" -Force
# 應該輸出新的版本號

# 3. 檢查文件權限
Get-Item "VersionInfo.cs" | Select-Object Mode, Owner
# 應該有 "rw" 權限

# 4. 查看版本號變化歷史
git log --oneline VersionInfo.cs | head -5
```

### 解決方案

```powershell
# 確保 VersionInfo.cs 格式正確
$correctContent = @"
public class VersionInfo
{
    public const string Version = "0.84.3";
}
"@
Set-Content "H:\MemberSystem\nobappGitHub\VersionInfo.cs" $correctContent -Encoding UTF8

# 重新執行發佈
dotnet publish NOBApp.csproj -c Release -p:PublishProfile=FolderProfile
```

---

## 問題 3: ZIP 檔案未建立

### 症狀
- 發佈完成，但 C:\BOT\ 中沒有 v*.zip 文件
- 看到 "✅ ZIP 檔案已建立" 消息，但實際不存在

### 原因分析

| 原因 | 徵兆 | 解決方案 |
|------|------|--------|
| 7-Zip 未安裝 | 看到 7z.exe 未找到的錯誤 | 安裝 7-Zip |
| 7-Zip 路徑不正確 | 命令找不到可執行文件 | 檢查路徑: `C:\Program Files\7-Zip\7z.exe` |
| 磁盤空間不足 | 7-Zip 返回錯誤代碼 | 清理磁盤，釋放空間 |
| 文件被鎖定 | 7-Zip 無法訪問文件 | 關閉應用程序或重啟 |
| 發佈目錄為空 | 沒有文件可以壓縮 | 檢查 C:\BOT\PS 中是否有文件 |

### 診斷步驟

```powershell
# 1. 檢查 7-Zip 是否安裝
Test-Path "C:\Program Files\7-Zip\7z.exe"
# 應該返回: True

# 2. 手動測試 7-Zip
& "C:\Program Files\7-Zip\7z.exe" a -tzip "C:\BOT\test.zip" "C:\BOT\PS\*"
# 應該成功完成

# 3. 檢查發佈目錄是否有文件
Get-ChildItem "C:\BOT\PS" | Measure-Object
# 應該返回大於 0 的數字

# 4. 檢查磁盤空間
Get-PSDrive C | Select-Object @{Name="Free(GB)";Expression={[math]::Round($_.Free/1GB,2)}}
# 應該有足夠的空間（至少 100 MB）

# 5. 查看 C:\BOT\ 中的所有 ZIP 文件
Get-ChildItem "C:\BOT\" -Filter "*.zip" -Recurse
```

### 解決方案

```powershell
# 安裝 7-Zip（需要管理員權限）
# 從 https://www.7-zip.org 下載並安裝

# 驗證安裝路徑
$7zipPath = "C:\Program Files\7-Zip\7z.exe"
if (Test-Path $7zipPath) {
    Write-Host "✅ 7-Zip 已正確安裝"
} else {
    Write-Host "❌ 7-Zip 安裝路徑不正確或未安裝"
}

# 手動建立 ZIP 文件（測試）
$version = "0.84.4"
$zipFile = "C:\BOT\v$version.zip"
& "C:\Program Files\7-Zip\7z.exe" a -tzip $zipFile "C:\BOT\PS\*"
```

---

## 問題 4: GitHub 上傳失敗

### 症狀
- ZIP 檔案已建立，但未上傳到 GitHub
- 看到 "❌ 上傳到 GitHub 時發生錯誤" 消息
- GitHub Release 未建立

### 原因分析

| 原因 | 錯誤信息 | 解決方案 |
|------|--------|--------|
| GitHub Token 未設定 | "⚠️ 未設定 GITHUB_TOKEN" | 設定環境變數 |
| GitHub Token 無效 | "401 Unauthorized" | 重新生成 Token |
| Token 權限不足 | "403 Forbidden" | 確保有 repo 權限 |
| Git 未安裝 | "找不到 Git" | 安裝 Git |
| Git 倉庫未初始化 | Git 命令失敗 | 初始化倉庫並設定 remote |
| 網絡連接問題 | 超時或連接拒絕 | 檢查網絡連接 |

### 診斷步驟

```powershell
# 1. 檢查 GitHub Token 是否設定
if ($env:GITHUB_TOKEN) {
    Write-Host "✅ Token 已設定: $($env:GITHUB_TOKEN.Substring(0,10))..."
} else {
    Write-Host "❌ Token 未設定"
}

# 2. 驗證 Token 有效性
$headers = @{
    "Authorization" = "token $env:GITHUB_TOKEN"
    "Accept" = "application/vnd.github.v3+json"
}

try {
    $user = Invoke-RestMethod -Uri "https://api.github.com/user" -Headers $headers
    Write-Host "✅ Token 有效，用戶: $($user.login)"
} catch {
    Write-Host "❌ Token 無效: $_"
}

# 3. 檢查 Git 是否可用
git --version
# 應該顯示 Git 版本

# 4. 檢查 Git 倉庫配置
cd "H:\MemberSystem\nobappGitHub"
git remote -v
# 應該顯示 GitHub 倉庫地址

# 5. 檢查是否有待提交的更改
git status
# 應該顯示提交狀態

# 6. 測試 GitHub API 連接
Invoke-RestMethod -Uri "https://api.github.com" | Select-Object -First 5
# 應該成功返回 API 信息
```

### 解決方案

```powershell
# 1. 設定 GitHub Token（如果未設定）
$env:GITHUB_TOKEN = "ghp_XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"

# 2. 驗證 Token 有效性
$headers = @{"Authorization" = "token $env:GITHUB_TOKEN"}
Invoke-RestMethod -Uri "https://api.github.com/user" -Headers $headers

# 3. 如果 Token 無效，重新生成
# 訪問: https://github.com/settings/tokens
# 點擊 "Generate new token (classic)"
# 設定權限: repo
# 複製新 Token 並重新設定

# 4. 檢查 Git 倉庫配置
cd "H:\MemberSystem\nobappGitHub"
git remote set-url origin "https://github.com/TwIcePenguin/nobapp.git"

# 5. 手動提交和推送
git add VersionInfo.cs
git commit -m "Test commit"
git push origin main
```

---

## 問題 5: 發佈速度很慢

### 症狀
- 發佈流程需要很長時間才能完成（> 5 分鐘）
- 看起來卡住或無響應

### 原因分析

| 原因 | 特徵 | 解決方案 |
|------|------|--------|
| ZIP 檔案過大 | 建立 ZIP 時卡住 | 排除不必要的文件 |
| 網絡連接慢 | GitHub 上傳慢 | 使用更快的網絡 |
| 磁盤 I/O 慢 | 整個流程都慢 | 使用 SSD 磁盤 |
| Git 倉庫大 | 推送緩慢 | 清理 Git 歷史 |

### 診斷步驟

```powershell
# 1. 測量各個步驟的時間
$start = Get-Date
dotnet publish NOBApp.csproj -c Release -p:PublishProfile=FolderProfile
$end = Get-Date
Write-Host "總耗時: $(($end - $start).TotalSeconds) 秒"

# 2. 檢查 ZIP 檔案大小
(Get-Item "C:\BOT\v*.zip" | Sort-Object LastWriteTime -Descending | Select-Object -First 1).Length / 1MB
# 應該在 50-200 MB 範圍內

# 3. 檢查網絡連接速度
Measure-Object -InputObject (Invoke-RestMethod "https://api.github.com" | ConvertTo-Json).Length -Sum

# 4. 檢查磁盤性能
Get-Disk | Where-Object BusType -eq "SATA" | Select-Object Number, BusType, MediaType
```

---

## 問題 6: PowerShell 執行策略限制

### 症狀
- 看到 "running scripts is disabled on this system" 錯誤
- PowerShell 腳本無法執行

### 原因
系統中樣的執行策略阻止了 PowerShell 腳本的執行

### 解決方案

```powershell
# 臨時解決（當前 PowerShell 會話）
powershell -ExecutionPolicy Bypass -File "script.ps1"

# 或在腳本調用中使用
powershell -NoProfile -ExecutionPolicy Bypass -File "script.ps1"

# 永久解決（需要管理員權限）
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser -Force

# 驗證設定
Get-ExecutionPolicy -List
```

---

## 完整診斷腳本

```powershell
# 運行此腳本進行完整診斷

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🔍 完整系統診斷" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 檢查所有必要工具
$checks = @(
    @{ Name = ".NET SDK"; Command = "dotnet --version" }
    @{ Name = "Git"; Command = "git --version" }
    @{ Name = "7-Zip"; Command = "Test-Path 'C:\Program Files\7-Zip\7z.exe'" }
    @{ Name = "GitHub Token"; Command = "if (`$env:GITHUB_TOKEN) { 'Set' } else { 'Not Set' }" }
)

foreach ($check in $checks) {
    Write-Host "檢查: $($check.Name)"
    try {
        $result = Invoke-Expression $check.Command
        if ($result) {
        Write-Host "✅ $($check.Name) 已找到"
    } else {
  Write-Host "⚠️ $($check.Name) 未找到"
  }
    } catch {
        Write-Host "❌ 檢查 $($check.Name) 時出錯: $_"
    }
    Write-Host ""
}

# 檢查項目文件
Write-Host "檢查項目文件："
$files = @(
    "H:\MemberSystem\nobappGitHub\NOBApp.csproj"
    "H:\MemberSystem\nobappGitHub\VersionInfo.cs"
  "H:\MemberSystem\nobappGitHub\UpdateVersion.ps1"
    "H:\MemberSystem\nobappGitHub\PostBuildScript.ps1"
)

foreach ($file in $files) {
 if (Test-Path $file) {
        Write-Host "✅ $([System.IO.Path]::GetFileName($file))"
    } else {
 Write-Host "❌ $([System.IO.Path]::GetFileName($file))"
    }
}

Write-Host ""
Write-Host "診斷完成"
```

---

## 快速修復清單

```
❌ 發佈卡住了
→ 嘗試: 重啟 Visual Studio，手動運行 dotnet publish

❌ 版本號未更新
→ 嘗試: 以管理員身份運行，檢查文件格式

❌ ZIP 檔案未建立
→ 嘗試: 安裝 7-Zip，檢查磁盤空間

❌ GitHub 上傳失敗
→ 嘗試: 驗證 Token，檢查網絡連接

❌ PowerShell 執行被阻止
→ 嘗試: 使用 -ExecutionPolicy Bypass 參數

❌ Git 命令失敗
→ 嘗試: 檢查 Git 是否安裝，倉庫是否初始化
```

---

## 獲取更多幫助

如果上述解決方案都無法解決問題，請收集以下信息：

1. 完整的發佈命令輸出（截圖或複製全文本）
2. PowerShell 中執行 `Get-ExecutionPolicy -List` 的輸出
3. PowerShell 中執行 `$env:GITHUB_TOKEN` 的結果
4. PowerShell 中執行 `git remote -v` 的結果
5. C:\BOT\ 目錄的文件列表
6. VersionInfo.cs 的完整內容
7. .csproj 中 `CustomActionsAfterPublish` 目標的完整內容

提供這些信息有助於快速定位問題。

---

**版本**: 1.0
**最後更新**: 2025-01-23
**狀態**: ✅ 完整指南
