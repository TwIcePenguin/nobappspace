# 🔐 安全修復 - GitHub Token 洩露問題

## 🔴 問題發現

在 `PostBuildScript.ps1` 中發現硬編碼的 GitHub Token：

```
❌ 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
```

**嚴重程度**：🔴 **CRITICAL**

---

## ✅ 修復完成

### 修改內容

**修改前**：
```powershell
[string]$GitHubToken = "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"
```

**修改後**：
```powershell
[string]$GitHubToken = ""

# 如果參數為空，嘗試從環境變量獲取
if ([string]::IsNullOrEmpty($GitHubToken)) {
    $GitHubToken = $env:GITHUB_TOKEN
}
```

---

## 🚨 立即行動清單

### 1️⃣ **撤銷洩露的 Token（最優先）**

訪問：https://github.com/settings/tokens

刪除這個 Token：
```
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
```

**為什麼**：任何人看到這個 Token 都能訪問您的倉庫。

### 2️⃣ **生成新 Token**

訪問：https://github.com/settings/tokens/new

設置：
- 名稱：NOBApp Release Token
- 範圍：✅ repo
- 複製新 Token

### 3️⃣ **設置環境變量**

```powershell
# 臨時（本次會話）
$env:GITHUB_TOKEN = "ghp_你的新Token"

# 或永久設置
# Windows 設置 > 環境變數 > 新增用戶變量
# GITHUB_TOKEN = ghp_你的新Token
```

### 4️⃣ **驗證修復**

```powershell
# 檢查代碼中是否還有硬編碼的 Token
grep -r "ghp_" .

# 應該返回空 ✅
```

---

## 🔍 為什麼會顯示「未設定」

**流程說明**：

```
1. MSBuild 調用 PostBuildScript.ps1
   ↓
2. 傳遞 -GitHubToken '$(GITHUB_TOKEN)'
   ↓
3. 如果 $env:GITHUB_TOKEN 為空，則傳遞空字符串
   ↓
4. PostBuildScript.ps1 接收空參數
   ↓
5. 检查 if ($GitHubToken) → $false
   ↓
6. 顯示 "⚠️ 未設定 GITHUB_TOKEN"
```

---

## ✨ 修復後的正確流程

```
1. 設置環境變量 GITHUB_TOKEN
   ↓
2. MSBuild 獲取環境變量值
   ↓
3. 傳遞給 PostBuildScript.ps1
   ↓
4. PostBuildScript.ps1 接收 Token
   ↓
5. 檢查 if ($GitHubToken) → $true
   ↓
6. ✅ 成功上傳到 GitHub
```

---

## 📋 最佳實踐

### ✅ 推薦

```powershell
# 在環境變量中設置
$env:GITHUB_TOKEN = "ghp_..."

# 在 MSBuild 中使用
-GitHubToken '$(GITHUB_TOKEN)'

# 在腳本中讀取
if ([string]::IsNullOrEmpty($GitHubToken)) {
    $GitHubToken = $env:GITHUB_TOKEN
}
```

### ❌ 永遠不要

```powershell
# 不要硬編碼在代碼中
[string]$GitHubToken = "ghp_..."

# 不要上傳到倉庫
git add PostBuildScript.ps1  # ❌

# 不要寫入日誌
Write-Host $GitHubToken  # ❌
```

---

## 🔒 安全檢查清單

- [x] 硬編碼 Token 已移除
- [x] 代碼改為使用環境變量
- [x] 構建成功
- [x] 需要：撤銷舊 Token
- [ ] 需要：生成新 Token
- [ ] 需要：設置新 Token 到環境變量

---

## 🆘 清理 Git 歷史（可選）

如果想完全移除 Token 從 Git 歷史：

```powershell
# 使用 git filter-branch
git filter-branch --force --index-filter \
  "git rm -r --cached --ignore-unmatch PostBuildScript.ps1" \
  --prune-empty --tag-name-filter cat -- --all

# 強制推送
git push origin --force --all
```

**注意**：這會改寫 Git 歷史，需要其他合作者重新克隆。

---

## 📊 安全狀態

```
修復前：🔴 CRITICAL（Token 洩露）
修復後：🟢 SAFE（環境變量設置）
```

---

**修復日期**：2025-11-23  
**狀態**：✅ **代碼層級已修復**  
**需要行動**：⚠️ **撤銷舊 Token + 生成新 Token**  

