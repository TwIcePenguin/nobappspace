# 🔐 GitHub Token - 正確設置指南

## ❌ 錯誤做法

```powershell
# ❌ 不要在代碼中硬編碼 Token！
[string]$GitHubToken = "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"
```

**風險**：Token 洩露，任何人都能訪問您的倉庫。

---

## ✅ 正確做法

### 方式 1：環境變量（臨時）

```powershell
# PowerShell 中設置（會話內有效）
$env:GITHUB_TOKEN = "ghp_你的新Token"

# 然後運行發佈
Build > Publish NOBApp...
```

### 方式 2：系統環境變量（永久）

Windows 11：
```
1. Win + X > 系統
2. 進階系統設定 > 環境變數
3. 新增用戶變量
   - 變量名：GITHUB_TOKEN
   - 變量值：ghp_你的新Token
4. 確定
5. 重啟 Visual Studio
```

### 方式 3：.env 文件（開發用）

創建 `.env` 文件（添加到 .gitignore）：

```
GITHUB_TOKEN=ghp_你的新Token
```

然後在腳本中讀取：

```powershell
if (Test-Path ".env") {
    Get-Content ".env" | ForEach-Object {
  $name, $value = $_.split('=')
        [Environment]::SetEnvironmentVariable($name, $value)
    }
}
```

---

## 🆘 如果 Token 已洩露

### 立即行動

1. **撤銷舊 Token**
   - 訪問：https://github.com/settings/tokens
   - 刪除 `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`

2. **生成新 Token**
   - 訪問：https://github.com/settings/tokens/new
   - 名稱：NOBApp Release Token
   - 範圍：repo
   - 複製新 Token

3. **設置新 Token**
   - 按上面的方式 1 或 2 設置環境變量

4. **清理 Git 歷史**
   - 如果想完全移除，使用：
   ```powershell
   git filter-branch --force --index-filter \
     "git rm -r --cached --ignore-unmatch PostBuildScript.ps1" \
     --prune-empty --tag-name-filter cat -- --all
   
   git push origin --force --all
   ```

---

## ✨ 驗證設置

```powershell
# 檢查環境變量是否設置
$env:GITHUB_TOKEN

# 應該輸出：ghp_你的新Token（不是空的）

# 運行發佈前檢查
.\StatusCheck.ps1
```

---

## 📋 檢查清單

發佈前：

- [ ] GitHub Token 已設置（`$env:GITHUB_TOKEN`）
- [ ] Token 不是硬編碼在代碼中
- [ ] Token 已添加到 .gitignore
- [ ] 舊 Token 已撤銷
- [ ] PostBuildScript.ps1 已更新

---

**安全級別**：🟢 **安全**

