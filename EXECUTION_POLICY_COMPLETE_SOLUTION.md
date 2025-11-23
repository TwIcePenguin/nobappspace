# 🔐 PowerShell 執行策略問題 - 完整解決方案

## ❌ 問題

```
CheckGitHubToken.ps1 : File H:\MemberSystem\nobappGitHub\CheckGitHubToken.ps1 
cannot be loaded because running scripts is disabled on this system
```

**原因**：Windows 默認禁用 PowerShell 腳本執行

---

## ✅ 三種解決方案

### 🥇 方案 1：最簡單 - 使用批處理文件（推薦）

**無需修改任何設置，直接雙擊運行！**

#### 新增的批處理文件

1. **PrePublishChecks.bat** ⭐ **推薦**
   - 執行完整的發佈前檢查
   - 同時檢查 Token 和硬編碼信息
   - 雙擊即可運行

2. **RunCheckToken.bat**
   - 單獨檢查 GitHub Token
   - 雙擊即可運行

3. **RunCheckForTokens.bat**
   - 單獨檢查硬編碼 Token
   - 雙擊即可運行

#### 使用方法

```
1. 在項目根目錄找到 PrePublishChecks.bat
2. 雙擊運行
3. 等待檢查完成
4. 按 Enter 關閉
```

---

### 🥈 方案 2：一行命令

無需修改系統設置，在 PowerShell 中運行：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\CheckGitHubToken.ps1"
```

或檢查硬編碼 Token：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\CheckForTokens.ps1"
```

---

### 🥉 方案 3：永久設置（需要管理員）

**一次性設置，之後可以直接運行所有 .ps1 腳本**

#### 步驟

1. **打開 PowerShell（管理員）**
   - Win + X
   - 選擇 Windows PowerShell（管理員）
   - 或者右鍵 PowerShell → 以管理員身份運行

2. **設置執行策略**
   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

3. **確認提示**
   - 輸入 `Y` 並按 Enter

4. **之後可以直接運行**
   ```powershell
   .\CheckGitHubToken.ps1
   .\CheckForTokens.ps1
   ```

#### 執行策略說明

| 策略 | 說明 | 推薦 |
|------|------|------|
| RemoteSigned | ✅ 允許本地腳本，下載腳本需要簽名 | ✅ |
| Unrestricted | ❌ 允許所有腳本（不安全） | ❌ |
| AllSigned | 所有腳本都需要簽名 | 企業環境 |
| Restricted | ❌ 不允許任何腳本（默認） | ❌ |

---

## 📊 方案對比

| 方案 | 難度 | 需要管理員 | 使用次數 | 推薦度 |
|------|------|-----------|---------|--------|
| 批處理文件 | ⭐ 最簡單 | ❌ 否 | 无限 | ⭐⭐⭐⭐⭐ |
| 一行命令 | ⭐⭐ 簡單 | ❌ 否 | 无限 | ⭐⭐⭐⭐ |
| 永久設置 | ⭐⭐⭐ 稍難 | ✅ 是 | 无限 | ⭐⭐⭐⭐ |

---

## 🚀 推薦的發佈流程

### 完全自動化方案（最簡單）

```
1. 雙擊 PrePublishChecks.bat
   ├─ 檢查 GitHub Token ✅
   ├─ 檢查硬編碼 Token ✅
   └─ 顯示結果

2. 按 Enter 關閉

3. 在 Visual Studio 中
   Build > Publish NOBApp...
```

### 命令行方案

```powershell
# 執行完整檢查
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\CheckGitHubToken.ps1"

# 或檢查硬編碼 Token
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\CheckForTokens.ps1"

# 然後發佈
Build > Publish NOBApp...
```

### 永久設置方案（推薦用於開發）

```powershell
# 一次性設置（管理員）
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# 之後可以直接運行
.\PrePublishChecks.bat
.\CheckGitHubToken.ps1
.\CheckForTokens.ps1
```

---

## 📁 新增文件

| 文件 | 類型 | 功能 |
|------|------|------|
| PrePublishChecks.bat | 批處理 | 完整發佈前檢查 ✅ |
| RunCheckToken.bat | 批處理 | Token 檢查 |
| RunCheckForTokens.bat | 批處理 | 硬編碼 Token 檢查 |
| POWERSHELL_QUICK_FIX.md | 文檔 | 快速解決指南 |
| POWERSHELL_EXECUTION_POLICY_FIX.md | 文檔 | 詳細技術說明 |

---

## ✨ 立即開始

### 最快的方式（30 秒）

```
1. 找到 PrePublishChecks.bat
2. 雙擊運行
3. 完成！
```

### 無需改動任何系統設置 ✅

批處理文件會自動繞過執行策略限制，無需修改 Windows 設置。

---

## 🎯 完整驗證流程

```
PrePublishChecks.bat
  ├─ ✅ GitHub Token 檢查
│   ├─ 環境變數設置
  │   ├─ Token 格式驗證
  │   ├─ API 連接測試
  │   └─ 倉庫訪問驗證
  │
  └─ ✅ 硬編碼 Token 檢查
   ├─ 掃描 .ps1 文件
      ├─ 掃描 .cs 文件
      ├─ 掃描 .xml 文件
      └─ 掃描 .xaml 文件
```

---

**狀態**：🟢 **完全就緒**

**推薦**：使用 `PrePublishChecks.bat` 🚀

