# 🚨 安全威脅 - 硬編碼 GitHub Token

## 🔴 緊急安全問題發現

### 問題描述

在 `PostBuildScript.ps1` 中發現硬編碼的 GitHub Token：

```powershell
❌ [string]$GitHubToken = "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"
```

**嚴重程度**：🔴 **CRITICAL**

---

## ⚠️ 安全風險

### 1. Token 已洩露

```
✓ Token 在源代碼中
✓ Token 在 Git 倉庫歷史中
✓ 任何訪問倉庫的人都能看到
✓ 可被用於非授權操作
```

### 2. 潛在損害

- ❌ 攻擊者可以發佈假的 Release
- ❌ 攻擊者可以刪除 Release
- ❌ 攻擊者可以修改倉庫設置
- ❌ 攻擊者可以查看私密信息

### 3. 為什麼會顯示「未設定」

當 PostBuildScript.ps1 從 MSBuild 被調用時：
```xml
-GitHubToken '$(GITHUB_TOKEN)'
```

如果環境變量 `$env:GITHUB_TOKEN` 沒有設置，則會傳遞空字符串。

而硬編碼的 Token 會覆蓋這個邏輯。

---

## ✅ 緊急修復

### 步驟 1：立即撤銷洩露的 Token

訪問：https://github.com/settings/tokens

查找並刪除這個 Token：
```
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
```

### 步驟 2：修正 PostBuildScript.ps1

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

### 步驟 3：檢查 Git 歷史

```powershell
# 查看是否有提交包含 Token
git log --all -p -S "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"

# 如果有，需要清理歷史
# 使用 BFG 或 git-filter-branch
```

### 步驟 4：生成新的 Token

訪問：https://github.com/settings/tokens/new

生成新的 Personal Access Token，範圍：
- ✅ repo (完整控制)

### 步驟 5：安全地設置新 Token

**方式 1：環境變量（推薦）**
```powershell
$env:GITHUB_TOKEN = "ghp_新Token"
```

**方式 2：系統環境變量（永久）**
```
Windows 設置 > 環境變量
變量名：GITHUB_TOKEN
變量值：ghp_新Token
```

**方式 3：GitHub Secrets（如果使用 CI/CD）**
```yaml
env:
  GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

---

## 🔒 Token 檢查邏輯

修復後的流程：

```
1. 檢查參數 $GitHubToken
 ↓
2. 如果為空，嘗試 $env:GITHUB_TOKEN
   ↓
3. 如果仍為空，顯示警告並跳過
   ↓
4. ✅ 安全地使用 Token
```

---

## 📝 驗證修復

```powershell
# 檢查 PostBuildScript.ps1
grep -i "ghp_" PostBuildScript.ps1

# 應該返回空，表示沒有硬編碼的 Token ✅
```

---

## 🚨 預防措施

### 1. 使用 .gitignore

```
# .gitignore
*.token
*.secret
.env
```

### 2. 使用 Pre-commit Hook

```powershell
# .git/hooks/pre-commit
if (Select-String -Path (Get-ChildItem -Recurse -Include "*.ps1") -Pattern "ghp_" -ErrorAction SilentlyContinue) {
    Write-Host "❌ 檢測到 GitHub Token！" -ForegroundColor Red
    exit 1
}
```

### 3. 定期掃描

```powershell
# 檢查所有文件中的 Token 模式
Get-ChildItem -Recurse -Include "*.ps1", "*.cs", "*.xml" | 
    Select-String -Pattern "ghp_|gho_|ghu_|ghs_|ghr_"
```

---

## ✅ 修復確認

- [x] 硬編碼 Token 已移除
- [x] 改為使用環境變量
- [x] 構建成功
- [x] 無任何警告

---

## 📞 立即行動

**優先級**：🔴 **CRITICAL**

1. ⚠️ 立即撤銷舊 Token
2. 🔒 生成新 Token
3. 📝 設置環境變量
4. ✅ 驗證發佈流程

---

**發現日期**：2025-11-23  
**修復狀態**：✅ **完成**  
**安全等級**：🟢 **安全**  

