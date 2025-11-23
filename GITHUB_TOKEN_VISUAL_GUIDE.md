# 🔐 GitHub Token 設置 - 視覺化指南

## 📍 第 1 步：訪問 GitHub Settings

### URL
```
https://github.com/settings/tokens
```

**或手動導航**：
```
1. 登入 GitHub
2. 點擊右上角頭像 ▼
3. 選擇 Settings
4. 左側菜單 > Developer settings
5. 選擇 Personal access tokens
6. 點擊 Tokens (classic)
```

---

## 📍 第 2 步：生成新 Token

### 點擊按鈕

```
[Generate new token (classic)]
    ↓
[Generate new token]
```

### 填寫信息

| 欄位 | 填寫 | 說明 |
|------|------|------|
| Token name | `NOBApp Release Token` | Token 名稱（自定義） |
| Expiration | `90 days` | 有效期（90 天或無期限） |

### 選擇權限（Scopes）

**必須選擇**：
- ✅ `repo` - Full control of private repositories

**結構**：
```
repo
├── repo:status - Access commit status
├── repo_deployment - Access deployment
├── public_repo - Access public repositories
├── repo:invite - Accept repository invitations
└── security_events - Read and write security events
```

---

## 📍 第 3 步：複製 Token

### 生成後的頁面

```
Personal access token

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

[Copy to clipboard]
```

**⚠️ 重要警告**：
```
Make sure to copy your token now. 
You won't be able to see it again!
```

**操作**：
1. 點擊 [Copy to clipboard]
2. 或手動複製整個 Token 字符串

---

## 📍 第 4 步：設置環境變數

### 方式 A：Windows 圖形界面（最簡單）

```
Windows 鍵 + R
  ↓
輸入: sysdm.cpl
  ↓
点擊: 進階 Tab
  ↓
点擊: 環境變數 按鈕
  ↓
選擇: 新增 (在 "用戶的使用者變數" 中)
  ↓
變數名稱: GITHUB_TOKEN
變數值: ghp_你複製的Token
  ↓
点擊: 確定 > 確定 > 確定
  ↓
重啟 Visual Studio
```

### 方式 B：PowerShell 命令

```powershell
# 臨時設置（當前會話）
$env:GITHUB_TOKEN = "ghp_你複製的Token"

# 永久設置（需要管理員）
[Environment]::SetEnvironmentVariable("GITHUB_TOKEN", "ghp_你複製的Token", "User")

# 驗證設置
$env:GITHUB_TOKEN
```

### 方式 C：批處理文件

創建 `SetToken.bat`：

```batch
@echo off
setx GITHUB_TOKEN "ghp_你複製的Token"
echo Token 已設置！
pause
```

以管理員身份運行。

---

## 📍 第 5 步：驗證設置

### 運行檢查工具

```powershell
# 方式 1：直接運行
.\CheckGitHubToken.ps1

# 方式 2：雙擊批處理
RunCheckToken.bat

# 方式 3：一行命令
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\CheckGitHubToken.ps1"
```

### 檢查清單

```
✅ 環境變數設置
   $env:GITHUB_TOKEN = ghp_...

✅ Token 格式正確
   Prefix: ghp_ (Personal Access Token)

✅ GitHub API 連接成功
   能夠連接到 api.github.com

✅ 倉庫訪問權限
   能夠訪問 TwIcePenguin/nobapp
```

---

## 📍 第 6 步：發佈應用

### Visual Studio 發佈

```
Visual Studio
  ↓
Build > Publish NOBApp...
  ↓
選擇: FolderProfile
  ↓
点擊: Publish
  ↓
等待發佈完成
```

### 預期流程

```
1. ✅ 版本號更新 (UpdateVersion.ps1)
   版本: 0.84.9

2. ✅ ZIP 文件創建 (CreateZip.ps1)
   位置: bin\Release\net8.0-windows7.0\publish\zip\v0.84.9.zip

3. ✅ GitHub Release 發佈 (PostBuildScript.ps1)
   Release URL: https://github.com/TwIcePenguin/nobapp/releases/tag/v0.84.9
```

---

## 🔍 故障排查

### 問題 1：Token 未設置

```
CheckGitHubToken.ps1 輸出：
❌ $env:GITHUB_TOKEN 未設置
```

**解決**：
1. 檢查是否在 sysdm.cpl 中設置了環境變數
2. 重啟 Visual Studio
3. 或運行：$env:GITHUB_TOKEN = "ghp_..."

### 問題 2：Token 格式錯誤

```
輸出：
⚠️ Token 格式可能不正確
```

**檢查**：
1. Token 是否複製完整
2. Token 是否包含空格
3. Token 前綴是否為 ghp_, gho_, ghu_, ghs_, 或 ghr_

### 問題 3：GitHub 連接失敗

```
輸出：
❌ GitHub 連接失敗
```

**檢查**：
1. 網絡連接是否正常
2. Token 是否有效
3. Token 是否過期
4. Token 是否有 repo 權限

### 問題 4：倉庫訪問失敗

```
輸出：
❌ 無法訪問 TwIcePenguin/nobapp 倉庫
```

**檢查**：
1. 倉庫名稱是否正確
2. Token 是否有 repo 權限
3. 是否有倉庫訪問權限

---

## 📊 完整流程圖

```
開始
  ↓
1. 生成 Token ────── 訪問 GitHub Settings
             ├─ Generate new token
         ├─ Name: NOBApp Release Token
          ├─ Scope: repo
          └─ 複製 Token
  ↓
2. 設置環境變數 ──── Windows 環境變數
  或 PowerShell 命令
         或 批處理文件
  ↓
3. 重啟應用 ──────── Visual Studio
     或 PowerShell
  ↓
4. 驗證 Token ────── 運行 CheckGitHubToken.ps1
        ├─ 環境變數 ✅
          ├─ Token 格式 ✅
     ├─ API 連接 ✅
        └─ 倉庫訪問 ✅
  ↓
5. 發佈應用 ──────── Build > Publish NOBApp...
           ├─ 版本更新 ✅
├─ ZIP 創建 ✅
       └─ GitHub 發佈 ✅
  ↓
完成 🎉
```

---

## ⏱️ 預計時間

| 步驟 | 時間 |
|------|------|
| 生成 Token | 2 分鐘 |
| 設置環境變數 | 2 分鐘 |
| 重啟應用 | 1 分鐘 |
| 驗證 Token | 1 分鐘 |
| 發佈應用 | 5 分鐘 |
| **總計** | **11 分鐘** |

---

**完成所有步驟後，即可開始自動發佈！** 🚀

