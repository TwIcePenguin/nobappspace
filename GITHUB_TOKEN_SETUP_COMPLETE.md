# 🔐 GitHub Token 設置完整指南

## 📋 目錄
1. [生成 Token](#生成-token)
2. [Windows 環境變數設置](#windows-環境變數設置)
3. [驗證 Token](#驗證-token)
4. [常見問題](#常見問題)

---

## 🔑 生成 Token

### 步驟 1：登入 GitHub

訪問 https://github.com 並登入你的帳戶

### 步驟 2：打開 Token 設置

1. 點擊右上角頭像 > **Settings**
2. 左側菜單選擇 **Developer settings**
3. 點擊 **Personal access tokens**
4. 選擇 **Tokens (classic)**

或直接訪問：https://github.com/settings/tokens

### 步驟 3：生成新 Token

1. 點擊 **Generate new token (classic)**
2. 點擊 **Generate new token**

### 步驟 4：配置 Token 權限

在 Token 頁面，填寫以下信息：

| 項目 | 設置 |
|------|------|
| Token name | `NOBApp Release Token` |
| Expiration | `No expiration` 或 `90 days` |
| Scopes | ✅ `repo` (完整倉庫控制) |

**核心權限說明**：
- ✅ `repo` - 完全控制公開和私有倉庫
  - `repo:status` - 訪問提交狀態
  - `repo_deployment` - 訪問部署
  - `public_repo` - 訪問公開倉庫
  - `repo:invite` - 接受倉庫邀請

### 步驟 5：複製 Token

```
ghp_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

**⚠️ 重要**：
- 複製完整的 Token 字符串
- 一旦離開此頁面，將無法再看到此 Token
- 如丟失，需要重新生成

---

## 🖥️ Windows 環境變數設置

### 方式 1：臨時設置（當前 PowerShell 會話）

在 PowerShell 中執行：

```powershell
$env:GITHUB_TOKEN = "ghp_你複製的Token"
```

例如：
```powershell
$env:GITHUB_TOKEN = "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"
```

**有效期**：
- ✅ 關閉 PowerShell 前一直有效
- ❌ 關閉 PowerShell 後失效

### 方式 2：永久設置（系統環境變數）

#### 方法 A：圖形界面（推薦新手）

1. **打開環境變數設置**
   - Win + R
   - 輸入 `sysdm.cpl`
   - 點擊 Enter

2. **進入高級選項**
   - 點擊 **進階** 標籤
   - 點擊 **環境變數** 按鈕

3. **新增用戶變數**
   - 在 "用戶的使用者變數" 區域
   - 點擊 **新增**

4. **填寫變數信息**
   - 變數名稱：`GITHUB_TOKEN`
   - 變數值：`ghp_你複製的Token`
   - 點擊 **確定**

5. **應用設置**
 - 連續點擊 **確定**
   - 重啟 Visual Studio 或 PowerShell

#### 方法 B：PowerShell 命令（快速）

以管理員身份運行 PowerShell，執行：

```powershell
# 設置當前用戶的環境變數
[Environment]::SetEnvironmentVariable("GITHUB_TOKEN", "ghp_你複製的Token", "User")

# 設置系統環境變數（需要管理員）
[Environment]::SetEnvironmentVariable("GITHUB_TOKEN", "ghp_你複製的Token", "Machine")
```

然後重啟 Visual Studio 或 PowerShell。

#### 方法 C：直接編輯環境變數

1. Win + R
2. 輸入 `rundll32.exe sysdm.cpl,EditEnvironmentVariables`
3. 在 "用戶的使用者變數" 中新增或修改 `GITHUB_TOKEN`

---

## ✅ 驗證 Token

### 方式 1：使用檢查工具（推薦）

```powershell
.\CheckGitHubToken.ps1
```

或雙擊：
```
RunCheckToken.bat
```

**預期輸出**：
```
1️⃣  檢查環境變數...
✅ $env:GITHUB_TOKEN 已設置
   Token 預覽: ghp_xxxxx...
   ✅ Token 格式正確 (Personal Access Token)

2️⃣  檢查 Git 配置...
✅ Git 用戶名: TwIcePenguin
✅ Git 郵箱: user@example.com

3️⃣  測試 GitHub API 連接...
✅ GitHub 連接成功！
   用戶: TwIcePenguin

4️⃣  檢查倉庫訪問權限...
✅ 可以訪問 TwIcePenguin/nobapp 倉庫
```

### 方式 2：PowerShell 命令

```powershell
# 查看 Token 是否設置
$env:GITHUB_TOKEN

# 如果有輸出則表示已設置，例如：
# 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
```

### 方式 3：測試 GitHub API

```powershell
# 設置 Headers
$headers = @{
 "Authorization" = "token $env:GITHUB_TOKEN"
    "Accept" = "application/vnd.github.v3+json"
}

# 測試連接
Invoke-RestMethod -Uri "https://api.github.com/user" -Headers $headers

# 應該返回你的 GitHub 用戶信息
```

---

## 📋 完整設置流程

### 快速 5 分鐘設置

```
1️⃣  生成 Token
   ├─ 訪問 https://github.com/settings/tokens
   ├─ Generate new token (classic)
   ├─ 名稱：NOBApp Release Token
   ├─ Scopes：✅ repo
   └─ 複製 Token

2️⃣  設置環境變數（二選一）
   
   選項 A - 臨時設置
   ├─ 打開 PowerShell
   └─ $env:GITHUB_TOKEN = "ghp_..."
   
   選項 B - 永久設置
   ├─ Win + R > sysdm.cpl
   ├─ 環境變數 > 新增
   ├─ GITHUB_TOKEN = "ghp_..."
   └─ 重啟 Visual Studio

3️⃣  驗證設置
   ├─ 雙擊 RunCheckToken.bat
   └─ 檢查結果

4️⃣  發佈應用
 └─ Build > Publish NOBApp...
```

---

## 🔍 檢查清單

發佈前檢查：

- [ ] 已訪問 https://github.com/settings/tokens
- [ ] 已生成新的 Personal Access Token
- [ ] Token 名稱：`NOBApp Release Token`
- [ ] Token 有 `repo` 權限
- [ ] 已複製 Token 字符串
- [ ] 已設置 `GITHUB_TOKEN` 環境變數
- [ ] 運行 `RunCheckToken.bat` 驗證成功
- [ ] 所有檢查都顯示 ✅

---

## 🆘 常見問題

### Q1：Token 已設置但還是顯示 (empty)

**原因**：環境變數未被應用

**解決**：
```powershell
# 查看是否設置
$env:GITHUB_TOKEN

# 如果為空，重新設置
$env:GITHUB_TOKEN = "ghp_你的Token"

# 驗證設置
$env:GITHUB_TOKEN
# 應該顯示你的 Token
```

### Q2：Token 過期了怎麼辦

**解決**：
1. 訪問 https://github.com/settings/tokens
2. 找到過期的 Token
3. 點擊 **Delete**
4. 生成新的 Token
5. 更新環境變數

### Q3：Token 洩露了怎麼辦

**立即行動**：
1. 訪問 https://github.com/settings/tokens
2. 找到洩露的 Token
3. 點擊 **Delete**
4. 生成新的 Token
5. 更新環境變數

### Q4：運行 CheckGitHubToken.ps1 失敗

**原因**：PowerShell 執行策略限制

**解決**：
```powershell
# 雙擊 RunCheckToken.bat（推薦，無需修改設置）

# 或使用命令
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\CheckGitHubToken.ps1"

# 或永久設置
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Q5：GitHub API 連接測試失敗

**檢查項**：
1. 網絡連接是否正常
2. Token 是否正確
3. Token 是否過期
4. Token 權限是否包含 `repo`

**解決**：
```powershell
# 檢查 Token 是否正確
$env:GITHUB_TOKEN

# 測試網絡
Test-NetConnection github.com -Port 443

# 手動測試 API
$headers = @{
 "Authorization" = "token $env:GITHUB_TOKEN"
    "Accept" = "application/vnd.github.v3+json"
}
Invoke-RestMethod -Uri "https://api.github.com/user" -Headers $headers
```

---

## 🔒 安全建議

1. **不要在代碼中硬編碼 Token**
   ```csharp
   // ❌ 不要這樣做
   string token = "ghp_xxxxx";
   
 // ✅ 使用環境變數
   string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
   ```

2. **定期檢查 Token**
   - 每 90 天更新一次
   - 檢查是否有異常活動

3. **使用強密碼**
 - GitHub 帳號密碼複雜度高
   - 啟用雙因素認證 (2FA)

4. **限制 Token 權限**
   - 只選擇需要的權限
   - 不要選擇超出需求的權限

---

## 📞 需要幫助

1. **查看 GitHub 文檔**
   - https://docs.github.com/en/authentication

2. **運行檢查工具**
   ```powershell
   .\CheckGitHubToken.ps1
   ```

3. **查看詳細日誌**
   - Visual Studio > Build > Output
   - 搜索 "GitHub" 或 "Token"

---

**設置完成後**，可以開始發佈應用到 GitHub Release！🚀

