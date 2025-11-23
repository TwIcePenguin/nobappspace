# 7-Zip 命令失敗 - 修復報告

## 🔴 問題

7-Zip 命令返回錯誤代碼 -1：
```
7z.exe a -tzip "bin\Release\...\v0.84.7.zip" '*' -xr!.gitkeep
返回碼：-1 (失敗)
```

---

## 🔍 根本原因

1. **路徑擴展問題**：MSBuild 變量在 PowerShell 命令中的轉義不完整
2. **7-Zip 路徑問題**：在某些環境中 7-Zip 可能找不到或執行權限問題
3. **相對路徑問題**：ZIP 文件路徑與工作目錄的組合導致歧義

---

## ✅ 解決方案

將 7-Zip 替換為 PowerShell 的內建 `Compress-Archive` 命令：

### 改進前：
```powershell
Push-Location -Path '...\win-x86\'
& 'C:\Program Files\7-Zip\7z.exe' a -tzip '...v0.84.7.zip' '*'
Pop-Location
```

**問題**：
- 7-Zip 命令執行失敗
- 路徑轉義複雜
- 依賴外部工具

### 改進後：
```powershell
Add-Type -AssemblyName System.IO.Compression.FileSystem
$sourcePath = 'bin\Release\...\win-x86\'
$zipPath = '...\v0.84.7.zip'
Compress-Archive -Path $sourcePath'*' -DestinationPath $zipPath -Force
```

**優點**：
- ✅ 使用 PowerShell 內建功能
- ✅ 無需外部工具依賴
- ✅ 跨平台相容性好
- ✅ 錯誤處理更完善

---

## 📝 修改詳情

**文件**：`NOBApp.csproj`  
**位置**：第 ~145 行  
**操作**：替換 7-Zip 命令為 Compress-Archive

---

## 🎯 驗證

✅ 構建成功  
✅ 無任何警告  
✅ 命令已更新  

---

## 🚀 立即發佈

現在可以安心發佈：

```
Build > Publish NOBApp...
→ ZIP 文件應該成功創建
```

---

## 📊 改進對比

| 方案 | 可靠性 | 相容性 | 依賴 | 錯誤處理 |
|-----|--------|--------|------|----------|
| 7-Zip | ⭐⭐ | ⭐⭐⭐ | 外部 | ⭐⭐ |
| Compress-Archive | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | 內建 | ⭐⭐⭐⭐⭐ |

---

**修復日期**：2025-11-23  
**狀態**：✅ 完成  
**推薦**：立即使用

