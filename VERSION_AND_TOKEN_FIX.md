# 🔧 版本號和 Token 檢查修復

## 問題 1：版本號少 1

### 原因

UpdateVersion.ps1 更新文件後，MSBuild 立即讀取文件，但文件可能還未完全寫入磁盤。

### 解決方案

在 `ReadLinesFromFile` 前添加 500ms 延遲：

```xml
<!-- 等待一下，確保文件寫入完成 -->
<Exec Command="powershell.exe -NoProfile -Command &quot;Start-Sleep -Milliseconds 500&quot;" />
```

### 修復位置

**NOBApp.csproj** 第 113-117 行

---

## 問題 2：grep 命令不可用

### 原因

`grep` 是 Linux/Unix 命令，在 Windows PowerShell 中不可用。

### 解決方案

創建 `CheckForTokens.ps1` - Windows PowerShell 原生腳本

### 使用方法

```powershell
# 在項目根目錄運行
.\CheckForTokens.ps1
```

### 功能

✅ 搜索所有 GitHub Token 模式：
- `ghp_` - Personal Access Token
- `gho_` - OAuth Token
- `ghu_` - User-to-Server Token
- `ghs_` - Server-to-Server Token
- `ghr_` - Refresh Token

✅ 搜索所有相關文件：
- PowerShell 腳本 (*.ps1)
- C# 代碼 (*.cs)
- XML 配置 (*.xml, *.csproj)
- XAML UI (*.xaml)

✅ 自動排除不需要的目錄：
- bin, obj, .git, node_modules, .vs

---

## ✅ 驗證修復

### 檢查版本號

```powershell
# 查看當前版本
(Get-Content VersionInfo.cs) -match 'Version'

# 輸出應為：v0.84.9（不再是 v0.84.8）
```

### 檢查 Token

```powershell
# 運行檢查腳本
.\CheckForTokens.ps1

# 預期輸出：✅ 未找到任何硬编码的 Token
```

---

## 📝 完整修復檢查清單

- [x] 版本號更新延遲已添加
- [x] CheckForTokens.ps1 已創建
- [x] 構建成功
- [x] 所有參數正確傳遞

---

**修復日期**：2025-11-23  
**狀態**：✅ **完成**

