# ⚡ 快速開始指南

## 🎯 当前状态

你的發佈流程已經配置完成。所有文件都已更新：

- ✅ `NOBApp.csproj` - 添加了 `CustomActionsAfterPublish` 目標
- ✅ `UpdateVersion.ps1` - 自動更新版本號
- ✅ `PostBuildScript.ps1` - GitHub Release 上傳
- ✅ `ManualPublish.bat` - 手動發佈批處理文件

## 📋 立即試試看

### 方法 1：命令行發佈（推薦）

打開 PowerShell（管理員），執行：

```powershell
# 設定 GitHub Token（可選，如果還沒設定）
$env:GITHUB_TOKEN = "你的_Personal_Access_Token"

# 進入項目目錄
cd "H:\MemberSystem\nobappGitHub"

# 執行發佈
dotnet publish NOBApp.csproj -c Release -p:PublishProfile=FolderProfile
```

### 方法 2：雙擊批處理文件

```
H:\MemberSystem\nobappGitHub\ManualPublish.bat
```

### 方法 3：Visual Studio 發佈

1. 右鍵點擊 **NOBApp** 項目
2. 選擇 **發佈**
3. 選擇 **FolderProfile**
4. 點擊 **發佈**

## 🔍 檢查執行結果

發佈完成後，查看：

### 1️⃣ 版本號是否增加了？
```powershell
Get-Content "H:\MemberSystem\nobappGitHub\VersionInfo.cs" | Select-String "Version"
```

應該看到類似：`public const string Version = "0.84.4";`

### 2️⃣ ZIP 文件是否建立了？
```powershell
Get-ChildItem "C:\BOT\" -Filter "v*.zip" | Sort-Object LastWriteTime -Descending
```

應該看到最新的 `v0.84.4.zip`

### 3️⃣ GitHub Release 是否建立了？
訪問：https://github.com/TwIcePenguin/nobapp/releases

應該看到新的 Release tag `v0.84.4`

## ⚠️ 如果某個步驟未執行

### 如果沒有更新版本號
- ❌ UpdateVersion.ps1 未執行
- 檢查：`Get-Content "H:\MemberSystem\nobappGitHub\VersionInfo.cs"`

### 如果沒有建立 ZIP 文件
- ❌ CustomActionsAfterPublish 未執行或 7-Zip 出錯
- 檢查：`Test-Path "C:\Program Files\7-Zip\7z.exe"`

### 如果未上傳到 GitHub
- ❌ PostBuildScript.ps1 未執行或 Token 未設定
- 檢查：`$env:GITHUB_TOKEN`
- 檢查：`git status`（是否有未提交的文件）

## 📝 完整的流程日誌示例

當你執行發佈時，應該看到類似的輸出：

```
========================================
🚀 開始執行自訂發佈後置工作
========================================
📁 PublishDir: C:\BOT\PS
📁 MSBuildProjectDirectory: H:\MemberSystem\nobappGitHub
⚙️  Configuration: Release

步驟 1️⃣  更新版本號
✅ 版本號: 0.84.4

🗑️  清理舊版本 ZIP 檔案...

步驟 2️⃣  打包應用程式
⬆️  建立 ZIP 檔案...
✅ ZIP 檔案已建立: C:\BOT\v0.84.4.zip

步驟 3️⃣  上傳到 GitHub Release
✅ 版本已提交
✅ 已推送到 GitHub
ℹ️  建立新的 Release v0.84.4
✅ ZIP 檔案已上傳

========================================
✅ 發佈後置工作完成
========================================
✅ 版本: v0.84.4
✅ Release URL: https://github.com/TwIcePenguin/nobapp/releases/tag/v0.84.4
✅ Download URL: https://github.com/TwIcePenguin/nobapp/releases/download/v0.84.4/v0.84.4.zip
```

## 🎓 三個關鍵要點

1. **版本號自動增加** - 每次發佈都會自動 +1（0.84.3 → 0.84.4）
2. **自動打包 ZIP** - 發佈時自動建立 `v0.84.4.zip`
3. **自動上傳 GitHub** - 建立 Release tag 和上傳 ZIP 文件

## 📞 需要幫助？

查看詳細指南：`PUBLISH_GUIDE.md`

或提供以下信息：
1. 完整的發佈輸出日誌
2. `C:\BOT\` 目錄的文件列表
3. `VersionInfo.cs` 的內容
4. `$env:GITHUB_TOKEN` 是否已設定
