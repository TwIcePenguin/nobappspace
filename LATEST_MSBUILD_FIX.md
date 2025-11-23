# ✨ 最新修復 - MSBuild 路徑轉義

## 🎉 又發現一個轉義問題 - 已修復！

### 問題症狀

```
❌ 未提供 ZIP 檔案路徑 (ZipPath)
```

CreateZip.ps1 沒有收到完整的 `$ZipPath` 參數。

---

## 🔍 根本原因

MSBuild `<Exec>` 中的反斜杠轉義：

```xml
<!-- ❌ 有問題 -->
-File &quot;script.ps1&quot; -ZipPath &quot;bin\Release\...\v0.84.9.zip&quot;
```

MSBuild 對 `&quot;` 內的反斜杠進行處理，導致路徑被破壞。

---

## ✅ 解決方案

### 改變策略：-File → -Command

```xml
<!-- ✅ 推薦 -->
-Command &quot;&amp; 'script.ps1' -ZipPath 'bin\Release\...\v0.84.9.zip'&quot;
```

**為什麼？**
- 單引號 `'` 在 MSBuild 中更安全
- `&` 操作符允許直接調用 PowerShell 腳本
- 避免 `-File` 的復雜轉義規則

---

## 🛠️ 修改項目

### NOBApp.csproj

從：
```xml
-File &quot;$(MSBuildProjectDirectory)\CreateZip.ps1&quot; -SourcePath &quot;$(PublishDir)&quot; -ZipPath &quot;$(ZipFilePath)&quot;
```

改為：
```xml
-Command &quot;&amp; '$(MSBuildProjectDirectory)\CreateZip.ps1' -SourcePath '$(PublishDir)' -ZipPath '$(ZipFilePath)'&quot;
```

### CreateZip.ps1

新增增強功能：
```powershell
# 修正參數
$SourcePath = $SourcePath.Trim()
$ZipPath = $ZipPath.Trim()

# 調試輸出
Write-Host "DEBUG: Raw ZipPath = '$ZipPath'" -ForegroundColor Gray
```

---

## 🚀 立即測試

```
Build > Publish NOBApp...
```

預期看到：
```
✅ ZIP 檔案已建立成功
📊 大小: XX.XX MB
```

不再出現 `❌ 未提供 ZIP 檔案路徑` 錯誤！

---

## 📚 相關文檔

- 📄 `MSBUILD_ESCAPING_FIX.md` - 詳細技術說明

---

**狀態**：🟢 **就緒**

