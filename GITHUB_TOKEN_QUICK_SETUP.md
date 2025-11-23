# ⚡ GitHub Token 設置 - 快速參考

## 🎯 30 秒快速設置

### 步驟 1：生成 Token

訪問：https://github.com/settings/tokens

點擊：**Generate new token (classic)**

配置：
- Name: `NOBApp Release Token`
- Scopes: ✅ `repo`

複製 Token（例如：`ghp_xxxxx...`）

### 步驟 2：設置環境變數

#### 方式 A - 臨時（當前會話）

```powershell
$env:GITHUB_TOKEN = "ghp_你的Token"
```

#### 方式 B - 永久（推薦）

```
Win + R > sysdm.cpl
進階 > 環境變數 > 新增
變數名稱：GITHUB_TOKEN
變數值：ghp_你的Token
重啟 Visual Studio
```

### 步驟 3：驗證

```powershell
.\CheckGitHubToken.ps1
```

或雙擊：`RunCheckToken.bat`

---

## 📋 各方式對比

| 方式 | 難度 | 時間 | 有效期 | 推薦 |
|------|------|------|--------|------|
| 臨時設置 | ⭐ | 30秒 | 本會話 | 快速測試 |
| 環境變數 | ⭐⭐ | 2分鐘 | 永久 | ⭐⭐⭐⭐⭐ |
| PowerShell 命令 | ⭐⭐ | 1分鐘 | 永久 | ⭐⭐⭐⭐ |

---

## ✅ 驗證結果

**成功**：
```
✅ $env:GITHUB_TOKEN 已設置
✅ Token 格式正確
✅ GitHub 連接成功
✅ 倉庫訪問正常
```

**失敗**：
```
❌ 檢查 Token 是否正確
❌ 檢查網絡連接
❌ 檢查 Token 是否過期
```

---

## 🚀 立即開始

```
1. 生成 Token: https://github.com/settings/tokens
2. 設置環境變數: $env:GITHUB_TOKEN = "ghp_..."
3. 驗證: .\CheckGitHubToken.ps1
4. 發佈: Build > Publish NOBApp...
```

---

**相關文檔**：`GITHUB_TOKEN_SETUP_COMPLETE.md`

