# PowerShell 執行策略解決方案

## 問題
```
CheckGitHubToken.ps1 : File ... cannot be loaded because running scripts is disabled on this system
```

---

## 🔧 解決方案

### 方案 1：臨時繞過（推薦用於一次性執行）

在 PowerShell 中執行：

```powershell
# 方式 A：直接繞過執行策略
powershell.exe -ExecutionPolicy Bypass -File ".\CheckGitHubToken.ps1"

# 方式 B：使用 NoProfile（更快）
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\CheckGitHubToken.ps1"

# 方式 C：在命令行中繞過
.\CheckGitHubToken.ps1 -ExecutionPolicy Bypass
```

---

### 方案 2：臨時設置當前會話

```powershell
# 設置當前 PowerShell 會話的執行策略
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process

# 然後運行腳本
.\CheckGitHubToken.ps1
```

**優點**：
- ✅ 只影響當前會話
- ✅ 關閉 PowerShell 後恢復
- ✅ 不需要管理員權限

---

### 方案 3：永久設置（需要管理員）

```powershell
# 以管理員身份打開 PowerShell，然後運行：
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# 或
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope LocalMachine
```

**執行策略選項**：
- `RemoteSigned`（推薦）：允許本地腳本，下載的腳本需要簽名
- `Unrestricted`：允許所有腳本（不推薦）
- `AllSigned`：所有腳本都需要簽名
- `Restricted`：不允許任何腳本（默認）

---

### 方案 4：批處理文件包裝器（最簡單）

創建 `RunCheckToken.bat`：

```batch
@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "CheckGitHubToken.ps1"
pause
```

然後雙擊 `RunCheckToken.bat` 運行（無需修改策略）

---

## 🚀 快速修復

### 最簡單的方式（一行命令）

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\CheckGitHubToken.ps1"
```

複製上面的命令，在 PowerShell 中粘貼並運行。

---

## 📋 建議的設置

### 對於開發環境

```powershell
# 以管理員身份運行 PowerShell，然後執行：
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

這樣設置後，你可以直接運行：
```powershell
.\CheckGitHubToken.ps1
.\CheckForTokens.ps1
```

---

## ✅ 驗證設置是否成功

運行以下命令查看當前執行策略：

```powershell
Get-ExecutionPolicy
```

應該返回 `RemoteSigned` 或 `Unrestricted`

---

## 🆘 如果還是不行

### 檢查 PowerShell 版本

```powershell
$PSVersionTable.PSVersion
```

應該是 PowerShell 5.0 或更高版本

### 使用 PowerShell ISE

如果桌面版 PowerShell 有問題，試試 PowerShell ISE：
1. Win + R
2. 輸入 `powershell_ise`
3. 打開腳本並執行

---

