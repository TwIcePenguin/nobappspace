# 🚀 NOBApp 發佈流程完整指南

## 問題分析

你的發佈流程中，`CustomActionsAfterPublish` 目標沒有被執行的原因可能是：

1. **Visual Studio 可能沒有顯示 MSBuild 詳細日誌** - 需要啟用詳細輸出
2. **發佈設定檔中的 `PublishSingleFile=true` 可能影響了 AfterTargets 觸發** - 已在 .csproj 中修復
3. **PowerShell 執行策略問題** - .csproj 中的 Exec 已添加 `-ExecutionPolicy Bypass`

## ✅ 解決方案

### 方案 A：使用命令行發佈（最可靠）

1. **打開 PowerShell**（以管理員身份運行）
2. **執行命令**：
```powershell
cd "H:\MemberSystem\nobappGitHub"
dotnet publish NOBApp.csproj -c Release -p:PublishProfile=FolderProfile
```

3. **預期輸出**：你會看到如下信息：
```
========================================
🚀 開始執行自訂發佈後置工作
========================================
📁 PublishDir: C:\BOT\PS
...
步驟 1️⃣  更新版本號
✅ 版本號: 0.84.4
...
步驟 2️⃣  打包應用程式
...
✅ ZIP 檔案已建立
...
步驟 3️⃣  上傳到 GitHub Release
...
✅ 發佈後置工作完成
========================================
```

### 方案 B：使用 Visual Studio（需要啟用詳細日誌）

1. **啟用 MSBuild 詳細日誌**：
   - 工具 → 選項 → 項目和解決方案 → 建置和執行
   - 將「MSBuild 項目建置輸出詳細程度」設為「詳細」或「診斷」

2. **執行發佈**：
   - 右鍵點擊項目 → 發佈
   - 選擇 "FolderProfile" 設定檔
   - 點擊發佈

3. **查看輸出**：
   - 檢查「輸出」窗口中的所有信息
   - 應該看到 `CustomActionsAfterPublish` 的執行

## 📋 預期的完整流程

### 執行時間順序：
1. ✅ MSBuild 開始 Publish 目標
2. ✅ 編譯應用程式
3. ✅ 發佈文件到 `C:\BOT\PS`
4. ✅ **觸發 CustomActionsAfterPublish**
   - 更新 `VersionInfo.cs` 版本號
   - 讀取新版本號
   - 刪除舊的 ZIP 文件
   - 建立新的 `v0.84.X.zip`
   - 上傳到 GitHub Release

### 文件變化：
- **VersionInfo.cs**：版本號增加（例如 0.84.3 → 0.84.4）
- **C:\BOT\PS\v0.84.4.zip**：新建
- **GitHub**：新建 Release tag `v0.84.4`，上傳 ZIP 文件

## 🔧 故障排除

### 如果沒有看到自訂工作執行的信息

**檢查清單**：
```
1. [ ] 是否看到 "🚀 開始執行自訂發佈後置工作" 消息？
2. [ ] 是否看到 "步驟 1️⃣  更新版本號" 消息？
3. [ ] VersionInfo.cs 中的版本號是否增加了？
4. [ ] C:\BOT\PS\ 目錄中是否有 v*.zip 文件？
5. [ ] GitHub Release 中是否有新的 Release？
```

### 如果仍未執行

**手動執行測試**：
```powershell
# 1. 在 PowerShell 中手動執行 Publish
cd "H:\MemberSystem\nobappGitHub"
dotnet publish NOBApp.csproj -c Release -p:PublishProfile=FolderProfile

# 2. 如果仍未看到自訂工作，檢查 .csproj 中是否有 CustomActionsAfterPublish 目標
# 3. 查看 .csproj 中的 <Target Name="CustomActionsAfterPublish" AfterTargets="Publish">
```

### 如果 ZIP 未建立

**檢查 7-Zip**：
```powershell
Test-Path "C:\Program Files\7-Zip\7z.exe"
# 應該返回 True

# 手動測試 7-Zip
& "C:\Program Files\7-Zip\7z.exe" a -tzip "C:\BOT\test.zip" "C:\BOT\PS\*"
```

### 如果未上傳到 GitHub

**檢查 GitHub Token**：
```powershell
# 檢查 Token 是否設定
$env:GITHUB_TOKEN

# 如果未設定，需要設定：
$env:GITHUB_TOKEN = "你的_Personal_Access_Token"

# 驗證 Token 有效性
$headers = @{"Authorization" = "token $env:GITHUB_TOKEN"}
Invoke-RestMethod -Uri "https://api.github.com/user" -Headers $headers
# 應該返回你的 GitHub 用戶信息
```

## 📝 快速檢查清單

```powershell
# 1. 檢查 dotnet
dotnet --version

# 2. 檢查 Git
git --version

# 3. 檢查 7-Zip
Test-Path "C:\Program Files\7-Zip\7z.exe"

# 4. 檢查 GitHub Token
if ($env:GITHUB_TOKEN) { "Token 已設定" } else { "Token 未設定" }

# 5. 檢查項目文件
Test-Path "H:\MemberSystem\nobappGitHub\NOBApp.csproj"

# 6. 檢查發佈設定檔
Test-Path "H:\MemberSystem\nobappGitHub\Properties\PublishProfiles\FolderProfile.pubxml"

# 7. 查看當前版本
Get-Content "H:\MemberSystem\nobappGitHub\VersionInfo.cs"
```

## 🎯 下一步操作

1. **開啟 PowerShell（管理員）**
2. **設定 GitHub Token**：
   ```powershell
   $env:GITHUB_TOKEN = "你的_token"
   ```
3. **執行發佈**：
   ```powershell
   cd "H:\MemberSystem\nobappGitHub"
   dotnet publish NOBApp.csproj -c Release -p:PublishProfile=FolderProfile
   ```
4. **觀察輸出日誌**，確認所有步驟都執行了
5. **檢查結果**：
   - ✅ VersionInfo.cs 版本號已增加
   - ✅ C:\BOT\ 有新的 v*.zip 文件
   - ✅ GitHub Release 有新的 tag 和 ZIP 文件

## 🆘 仍有問題？

如果按照上述步驟後仍然無法執行，請提供以下信息：

1. 完整的 Publish 命令輸出（截圖或複製全文本）
2. `.csproj` 中 `CustomActionsAfterPublish` 目標的內容
3. C:\BOT\ 目錄的內容（`ls C:\BOT\`）
4. GitHub Token 的前 10 個字符和是否可用

---

**版本**: 1.0  
**更新日期**: 2025-01-23  
**適用範圍**: NOBApp Release 0.84.3+
