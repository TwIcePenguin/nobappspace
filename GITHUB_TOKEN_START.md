# 🎯 GitHub Token 設置 - 完全指南

## ⚡ 超快速版（3 分鐘）

### 1️⃣ 生成 Token
```
訪問: https://github.com/settings/tokens
點擊: Generate new token (classic)
選擇: repo
複製: Token 字符串
```

### 2️⃣ 設置環境變數

**選項 A - 臨時**
```powershell
$env:GITHUB_TOKEN = "ghp_你的Token"
```

**選項 B - 永久**（推薦）
```
Win + R > sysdm.cpl
進階 > 環境變數 > 新增
名稱：GITHUB_TOKEN
值：ghp_你的Token
重啟 Visual Studio
```

### 3️⃣ 驗證
```powershell
.\CheckGitHubToken.ps1
# 或
RunCheckToken.bat
```

---

## 📚 詳細指南

| 文檔 | 內容 | 推薦 |
|------|------|------|
| GITHUB_TOKEN_QUICK_SETUP.md | 快速參考卡 | ⭐⭐⭐⭐⭐ |
| GITHUB_TOKEN_VISUAL_GUIDE.md | 視覺化步驟 | ⭐⭐⭐⭐⭐ |
| GITHUB_TOKEN_SETUP_COMPLETE.md | 完整詳細版 | ⭐⭐⭐ |

---

## 🔗 重要鏈接

| 說明 | 鏈接 |
|------|------|
| 生成 Token | https://github.com/settings/tokens |
| GitHub 文檔 | https://docs.github.com/en/authentication |
| 我的倉庫 | https://github.com/TwIcePenguin/nobapp |

---

## ✅ 完成清單

- [ ] 訪問 GitHub Settings
- [ ] 生成新 Token
- [ ] 複製 Token 字符串
- [ ] 設置環境變數 (GITHUB_TOKEN)
- [ ] 重啟 Visual Studio
- [ ] 運行 CheckGitHubToken.ps1 驗證
- [ ] 檢查結果全部 ✅
- [ ] 準備發佈！

---

## 🚀 下一步

```
1. 完成 Token 設置
2. 運行: .\CheckGitHubToken.ps1
3. 確認所有檢查 ✅
4. 執行: Build > Publish NOBApp...
5. 等待自動發佈完成
6. 查看 GitHub Release
```

---

**狀態**：🟢 **準備就緒**

