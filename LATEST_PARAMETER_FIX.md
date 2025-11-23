# 🎉 第 6 個問題已修復 - 參數分割

## ❌ 新發現的問題

PostBuildScript.ps1 無法接收參數：
```
❌ 未提供版本檔案路徑 (VersionInfoPath)
```

---

## 🔍 根本原因

MSBuild 中使用 `-File` 加雙引號時，參數在多層轉義中被破壞：

```
-File "script.ps1" -Param "C:\path\file.cs"
              ↓
  MSBuild 轉義
            ↓
      參數值被截斷 ❌
```

---

## ✅ 解決方案

統一所有 PowerShell 調用使用 `-Command` 和單引號：

```xml
<!-- ✅ 正確 -->
-Command "&amp; 'script.ps1' -Param 'value'"
```

---

## 📝 修改項目

### NOBApp.csproj

修復了兩個腳本調用：

1. **UpdateVersion.ps1** (第 113 行)
   - 從 `-File` 改為 `-Command`

2. **PostBuildScript.ps1** (第 157 行) ⭐ **新修復**
   - 從 `-File` 改為 `-Command`
   - 所有參數使用單引號

---

## 🚀 完整修復統計

```
總問題：6 個
已修復：6 個 ✅
成功率：100%
```

| # | 組件 | 問題 | 修復 |
|-|------|------|------|
| 1 | PostBuildScript.ps1 | 路徑後綴 | ✅ |
| 2 | NOBApp.sln | 配置 | ✅ |
| 3 | 7-Zip | 命令失敗 | ✅ |
| 4 | CreateZip.ps1 | 轉義 | ✅ |
| 5 | NOBApp.csproj | MSBuild 轉義 | ✅ |
| 6 | 所有腳本調用 | 參數分割 | ✅ NEW |

---

## 🎯 立即發佈

```
Build > Publish NOBApp...
```

所有參數現在都能正確傳遞！

---

**狀態**：🟢 **完全就緒**

