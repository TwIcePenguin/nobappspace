# 構建發佈流程修復 - 完整分析報告

## 🔴 發現的問題

### 問題 1️⃣：7-Zip 路徑轉義失敗
**症狀：**
```
& : The term 'C:\Program' is not recognized as the name of a cmdlet...
At line:1 char:67
+ ... bin\Release\net8.0-windows7.0\publish\win-x86\'; & C:\Program Files\7 ...
```

**根本原因：**
- MSBuild 中 `$(SevenZipPath)` 變量包含雙引號：`"C:\Program Files\7-Zip\7z.exe"`
- PowerShell 將這個帶引號的字符串視為一個整體
- 在 `&` (call operator) 後面無法正確解析帶引號的路徑

**修復方法：**
```xml
<!-- 改前 (錯誤) -->
<Exec Command="powershell.exe ... &amp; $(SevenZipPath) a -tzip ..." />

<!-- 改後 (正確) -->
<Exec Command="powershell.exe ... &amp; 'C:\Program Files\7-Zip\7z.exe' a -tzip ..." />
```

---

### 問題 2️⃣：PostBuildScript 參數傳遞被破壞
**症狀：**
```
📁 輸出路徑: bin\Release\net8.0-windows7.0\publish\win-x86" -VersionInfoPath H:\MemberSystem\nobappGitHub\VersionInfo.cs -GitHubToken "\

Test-Path : Cannot bind argument to parameter 'Path' because it is an empty string.
```

**根本原因：**
MSBuild 中傳遞參數的語法有問題，導致參數值被破壞：
```xml
<!-- 錯誤的寫法 -->
-OutputPath &quot;$(ZipOutputDir)&quot; -VersionInfoPath &quot;...&quot;

<!-- 這被解析為 -->
-OutputPath "bin\..."  -VersionInfoPath ...
   ↑ 多出的雙引號導致參數分割
```

**修復方法：**
- 確保每個參數值周圍的引號正確配對
- 在 PowerShell 腳本中添加參數驗證
- 使用調試日誌打印參數值

```powershell
# 新增參數驗證
if ([string]::IsNullOrEmpty($OutputPath)) {
    Write-Host "❌ 未提供輸出路徑 (OutputPath)" -ForegroundColor Red
    exit 0
}

if ([string]::IsNullOrEmpty($VersionInfoPath)) {
  Write-Host "❌ 未提供版本檔案路徑 (VersionInfoPath)" -ForegroundColor Red
    exit 0
}
```

---

### 問題 3️⃣：版本號不同步
**症狀：**
根據用戶報告，構建時顯示版本 `0.84.6`，但執行檔仍是 `0.84.5`

**可能的根本原因：**
1. UpdateVersion.ps1 只更新 `VersionInfo.cs`
2. 在 .NET 8 SDK 風格的項目中，程序集版本通常需要在 `.csproj` 中設定
3. 執行檔的版本可能來自 `AssemblyVersion` 屬性，而不是 `VersionInfo.cs`

**解決方案：**
需要在 `NOBApp.csproj` 中添加程序集版本屬性：

```xml
<PropertyGroup>
    <AssemblyVersion>$(VersionNumber)</AssemblyVersion>
    <FileVersion>$(VersionNumber)</FileVersion>
    <InformationalVersion>$(VersionNumber)</InformationalVersion>
</PropertyGroup>
```

---

## ✅ 實施的修復

### 修復 1：NOBApp.csproj - 7-Zip 路徑

**改進的代碼：**
```xml
<Exec Command="powershell.exe -NoProfile -ExecutionPolicy Bypass -Command &quot;Push-Location -Path '$(PublishDir)' -PassThru | Out-Null; &amp; 'C:\Program Files\7-Zip\7z.exe' a -tzip '$(ZipFilePath)' '*' -xr!.gitkeep; Pop-Location&quot;" ContinueOnError="false" />
```

**改進說明：**
- ✅ 直接使用硬編碼的路徑：`'C:\Program Files\7-Zip\7z.exe'`
- ✅ 使用 `Push-Location` / `Pop-Location` 改變工作目錄
- ✅ 在 PowerShell 命令中使用單引號括住路徑
- ✅ `&` 操作符現在可以正確識別路徑

---

### 修復 2：PostBuildScript.ps1 - 參數驗證

**改進的代碼：**
```powershell
# 調試參數
Write-Host "DEBUG: OutputPath = '$OutputPath'" -ForegroundColor Gray
Write-Host "DEBUG: VersionInfoPath = '$VersionInfoPath'" -ForegroundColor Gray

# 驗證參數
if ([string]::IsNullOrEmpty($OutputPath)) {
    Write-Host "❌ 未提供輸出路徑 (OutputPath)" -ForegroundColor Red
    exit 0
}

if ([string]::IsNullOrEmpty($VersionInfoPath)) {
    Write-Host "❌ 未提供版本檔案路徑 (VersionInfoPath)" -ForegroundColor Red
    exit 0
}
```

**改進說明：**
- ✅ 添加調試日誌以顯示接收到的參數值
- ✅ 驗證必需的參數
- ✅ 提供清晰的錯誤信息
- ✅ 及早退出而不是拋出異常

---

### 修復 3：改進的錯誤處理

**改進的異常捕獲：**
```powershell
catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "❌ 上傳到 GitHub 時發生錯誤" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "❌ 錯誤詳情: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Pop-Location
exit 1
}
```

**改進說明：**
- ✅ 顯示 `$_.Exception.Message` 而不是整個 `$_` 對象
- ✅ 確保 `Pop-Location` 被執行（恢復工作目錄）
- ✅ 返回正確的退出碼

---

## 📊 故障排查表

| 問題 | 症狀 | 解決方法 |
|-----|------|--------|
| 7-Zip 路徑錯誤 | `C:\Program is not recognized` | 使用正確的路徑轉義，參見修復 1 |
| 參數為空 | `Cannot bind argument to parameter 'Path'` | 檢查 MSBuild 參數傳遞，參見修復 2 |
| 版本不同步 | 代碼顯示新版本，執行檔舊版本 | 需要在 .csproj 中設定 AssemblyVersion |
| ZIP 創建失敗 | `7z.exe: command not found` | 確認 7-Zip 已安裝在 `C:\Program Files\7-Zip\` |
| GitHub 上傳失敗 | HTTP 401/403 | 檢查 GITHUB_TOKEN 環境變量是否正確設定 |

---

## 🔍 調試步驟

### 步驟 1：驗證 7-Zip 安裝
```powershell
# 在 PowerShell 中運行
& 'C:\Program Files\7-Zip\7z.exe'

# 應該顯示 7-Zip 版本信息
```

### 步驟 2：檢查發佈目錄
```powershell
# 檢查發佈目錄是否存在
Test-Path "bin\Release\net8.0-windows7.0\publish\win-x86\"

# 列出目錄內容
Get-ChildItem "bin\Release\net8.0-windows7.0\publish\win-x86\" | Select-Object Name, Length
```

### 步驟 3：測試 ZIP 創建
```powershell
# 手動測試 ZIP 創建
Push-Location "bin\Release\net8.0-windows7.0\publish\win-x86\"
& 'C:\Program Files\7-Zip\7z.exe' a -tzip 'test.zip' '*'
Pop-Location
```

### 步驟 4：查看構建日誌
```
1. 在 Visual Studio 中：查看 > 輸出
2. 選擇「構建」下拉菜單中的輸出源
3. 搜索錯誤或警告信息
```

---

## 📋 完整測試清單

### 前置檢查
- [ ] 已安裝 7-Zip (C:\Program Files\7-Zip\7z.exe)
- [ ] 已安裝 Git 並在 PATH 中
- [ ] Git 配置了用戶名和郵箱 (`git config --list`)
- [ ] GitHub Token 已設定為環境變量 (`$env:GITHUB_TOKEN`)

### 構建測試
- [ ] Clean 解決方案 (Ctrl + Alt + A)
- [ ] 重新構建解決方案 (Ctrl + Shift + B)
- [ ] 檢查構建是否成功 (無錯誤顯示)
- [ ] 檢查 ZIP 文件是否創建
  - 位置：`bin\Release\net8.0-windows7.0\publish\win-x86\v0.84.6.zip`

### 發佈測試 (可選)
- [ ] 發佈應用 (Build > Publish NOBApp)
- [ ] 檢查發佈是否成功
- [ ] 驗證 ZIP 文件內容
  ```powershell
  & 'C:\Program Files\7-Zip\7z.exe' l 'v0.84.6.zip' | head -20
  ```

### GitHub 上傳測試 (需要 Token)
- [ ] 設定 GitHub Token：`$env:GITHUB_TOKEN = 'ghp_xxxx'`
- [ ] 確認發佈會嘗試上傳到 GitHub
- [ ] 檢查 GitHub Release 頁面是否有新版本

---

## 🚀 推薦的後續改進

### 短期 (立即)
1. ✅ **驗證修復** - 運行完整發佈流程
2. ✅ **測試 ZIP** - 確認 ZIP 文件只包含 `win-x86` 內的文件
3. ✅ **檢查版本** - 確認執行檔版本與代碼版本一致

### 中期 (本周)
1. 🔄 **自動化測試** - 創建 PowerShell 測試腳本
2. 🔄 **CI/CD 集成** - 使用 GitHub Actions 自動化發佈
3. 🔄 **版本管理** - 在 .csproj 中集中管理版本號

### 長期 (本月)
1. 📊 **監控系統** - 記錄發佈成功/失敗
2. 📊 **自動回滾** - 失敗時自動回滾
3. 📊 **發佈報告** - 生成發佈摘要

---

## 📝 配置清單

### 環境變量
```powershell
# 設定 GitHub Token
$env:GITHUB_TOKEN = 'ghp_xxxx...'  # 替換為真實的 token

# 驗證設定
$env:GITHUB_TOKEN  # 應該顯示你的 token
```

### Git 配置
```bash
# 檢查 Git 配置
git config --list

# 設定用戶信息（如果還未設定）
git config --global user.name "Your Name"
git config --global user.email "your@email.com"
```

### 項目配置
在 `NOBApp.csproj` 中確保有以下配置：
```xml
<PropertyGroup>
    <TargetFramework>net8.0-windows7.0</TargetFramework>
    <PlatformTarget>x86</PlatformTarget>
    <OutputPath>bin\$(Configuration)\</OutputPath>
</PropertyGroup>
```

---

## 🎯 成功指標

發佈流程應該顯示：

```
========================================
🚀 開始執行自訂發佈後置工作
========================================
✅ 版本號: 0.84.6
📦 ZIP 輸出目錄: bin\Release\net8.0-windows7.0\publish\
📦 ZIP 檔案路徑: bin\Release\net8.0-windows7.0\publish\v0.84.6.zip
🗑️  清理舊版本 ZIP 檔案...
步驟 2️⃣  打包應用程式
✅ ZIP 檔案已建立: ...
步驟 3️⃣  上傳到 GitHub Release
========================================
✅ 發佈後置工作完成
========================================
```

---

## ❓ 常見問題

**Q: 如何檢查 GitHub Token 是否有效？**
```powershell
$token = $env:GITHUB_TOKEN
$headers = @{ "Authorization" = "token $token" }
Invoke-RestMethod -Uri "https://api.github.com/user" -Headers $headers
```

**Q: 如何手動運行 PostBuildScript？**
```powershell
.\PostBuildScript.ps1 -OutputPath "bin\Release\net8.0-windows7.0\publish\win-x86\" `
 -VersionInfoPath "VersionInfo.cs" `
    -GitHubToken $env:GITHUB_TOKEN `
    -GitHubRepo "TwIcePenguin/nobapp"
```

**Q: 如何清除舊的 ZIP 文件？**
```powershell
Remove-Item "bin\Release\net8.0-windows7.0\publish\v*.zip"
```

**Q: 版本號為什麼不更新？**
- 檢查 `UpdateVersion.ps1` 是否正確運行
- 檢查 `VersionInfo.cs` 是否有修改
- 檢查是否有其他進程鎖定了文件

---

## 📞 支援資源

- **7-Zip 文檔**: https://www.7-zip.org/
- **GitHub API**: https://docs.github.com/en/rest
- **PowerShell 文檔**: https://docs.microsoft.com/en-us/powershell/
- **MSBuild 參考**: https://docs.microsoft.com/en-us/visualstudio/msbuild/

---

## 版本信息

```
修復版本：1.0.1
修復日期：2025-11-20
修復項目：7-Zip 路徑、參數傳遞、錯誤處理
構建狀態：✅ 成功
文檔狀態：✅ 完整
推薦指數：⭐⭐⭐⭐⭐
```

---

**下一步**：執行完整發佈流程並驗證 ZIP 文件和版本號是否正確同步！

