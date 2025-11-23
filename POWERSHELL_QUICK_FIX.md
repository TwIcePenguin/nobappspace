# 🚀 PowerShell 執行策略 - 快速解決方案

## ❌ 問題

```
CheckGitHubToken.ps1 cannot be loaded because running scripts is disabled on this system
```

## ✅ 解決方案

### 方案 A：最簡單 - 雙擊批處理文件

在項目根目錄找到這些文件，直接雙擊即可運行：

1. **RunCheckToken.bat** - 檢查 GitHub Token
2. **RunCheckForTokens.bat** - 檢查硬編碼 Token
3. **PrePublishChecks.bat** - 完整檢查清單（推薦）

✅ **無需修改執行策略，無需管理員權限**

---

### 方案 B：一行命令

在 PowerShell 中複製並粘貼：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\CheckGitHubToken.ps1"
```

---

### 方案 C：永久解決（需要管理員）

1. 以管理員身份打開 PowerShell
2. 運行：
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```
3. 輸入 `Y` 確認
4. 之後可以直接運行：`.\CheckGitHubToken.ps1`

---

## 🎯 推薦步驟

### 快速檢查（無需設置）

```
1. 雙擊 PrePublishChecks.bat
2. 等待檢查完成
3. 按 Enter 關閉
```

### 永久設置（一次性）

```
1. 右鍵點擊 PowerShell → 以管理員身份運行
2. 執行：Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
3. 輸入 Y 確認
4. 之後可以直接運行 .ps1 腳本
```

---

## 📋 新增的工具

| 文件 | 功能 | 使用方式 |
|------|------|---------|
| RunCheckToken.bat | 檢查 Token | 雙擊運行 |
| RunCheckForTokens.bat | 檢查硬編碼 Token | 雙擊運行 |
| PrePublishChecks.bat | 完整檢查 | 雙擊運行 ✅ |

---

**推薦**：使用 `PrePublishChecks.bat` 進行發佈前檢查 ✅

