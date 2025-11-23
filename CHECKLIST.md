# ✅ 最終檢查清單

## 已完成的配置

### 1. 核心文件
- ✅ **NOBApp.csproj** - 添加 `CustomActionsAfterPublish` 目標
- ✅ **UpdateVersion.ps1** - 自動更新版本號的 PowerShell 腳本
- ✅ **PostBuildScript.ps1** - GitHub Release 上傳的 PowerShell 腳本
- ✅ **VersionInfo.cs** - 版本號定義文件

### 2. 輔助文件
- ✅ **ManualPublish.bat** - 手動發佈批處理文件
- ✅ **TestPublish.ps1** - 發佈流程測試腳本
- ✅ **DiagnosticCheck.ps1** - 環境診斷腳本
- ✅ **PUBLISH_GUIDE.md** - 詳細發佈指南
- ✅ **QUICK_START.md** - 快速開始指南

### 3. 項目配置
- ✅ 發佈設定檔: `FolderProfile.pubxml`
- ✅ 發佈目錄: `C:\BOT\PS`
- ✅ 配置: Release + x86

## 環境要求

### 必須安裝
- ✅ .NET 8 SDK (dotnet 命令可用)
- ✅ Git (git 命令可用)
- ✅ 7-Zip (`C:\Program Files\7-Zip\7z.exe`)

### 必須配置
- ✅ GitHub Personal Access Token (設定為環境變數 `GITHUB_TOKEN`)
- ✅ Git 倉庫已初始化
- ✅ Git 遠端指向 `https://github.com/TwIcePenguin/nobapp`

## 完整的發佈流程

### 執行方式
選擇以下任一種：

1. **命令行**：
   ```powershell
   dotnet publish NOBApp.csproj -c Release -p:PublishProfile=FolderProfile
   ```

2. **Visual Studio**：
   - 右鍵點擊項目 → 發佈 → FolderProfile → 發佈

3. **批處理**：
   - 雙擊 `ManualPublish.bat`

### 自動執行的步驟

#### 📋 步驟 1: 編譯和發佈（由 MSBuild 執行）
```
dotnet publish 執行
↓
編譯應用程式
↓
發佈文件到 C:\BOT\PS
↓
觸發 CustomActionsAfterPublish
```

#### 🔄 步驟 2: 更新版本號（由 UpdateVersion.ps1 執行）
```
讀取 VersionInfo.cs
↓
提取當前版本號 (例: 0.84.3)
↓
增加最後一位 (0.84.3 → 0.84.4)
↓
寫回 VersionInfo.cs
```

#### 📦 步驟 3: 打包應用程式（由 7-Zip 執行）
```
刪除舊的 ZIP 文件
↓
遍歷 C:\BOT\PS 中的所有文件
↓
建立 v0.84.4.zip
↓
存儲到 C:\BOT\
```

#### 📤 步驟 4: 上傳到 GitHub（由 PostBuildScript.ps1 執行）
```
讀取版本號 0.84.4
↓
提交 VersionInfo.cs 變更
↓
推送到 GitHub main 分支
↓
建立 Release tag v0.84.4
↓
上傳 v0.84.4.zip 到 Release
↓
完成
```

## 文件變化跟蹤

### 每次發佈後應該看到的變化

**本地文件系統**:
```
C:\BOT\
├── PS\       (發佈目錄)
│   ├── NOBApp.exe
│   ├── *.dll
│   └── ...
└── v0.84.4.zip            ✨ 新建立
```

**VersionInfo.cs**:
```csharp
public class VersionInfo
{
    public const string Version = "0.84.4";  // ✨ 自動更新
}
```

**GitHub Release**:
```
Repository: TwIcePenguin/nobapp
↓
新 Release tag: v0.84.4         ✨ 自動建立
├── Release 名稱: Release v0.84.4
├── Release 說明: 自動化發佈說明
└── 附件: v0.84.4.zip           ✨ 自動上傳
    ├── 文件名: v0.84.4.zip
    ├── 大小: ~XX MB
    └── 下載 URL: https://github.com/TwIcePenguin/nobapp/releases/download/v0.84.4/v0.84.4.zip
```

## 命令行快速參考

```powershell
# 1. 設定 GitHub Token
$env:GITHUB_TOKEN = "ghp_xxxxxxxxxxxxxxxxxxxxxxxxxxxxx"

# 2. 進入項目目錄
cd "H:\MemberSystem\nobappGitHub"

# 3. 執行發佈
dotnet publish NOBApp.csproj -c Release -p:PublishProfile=FolderProfile

# 4. 驗證版本
Get-Content VersionInfo.cs | Select-String "Version"

# 5. 驗證 ZIP 文件
Get-ChildItem "C:\BOT\" -Filter "v*.zip" -Descending -Top 1 | Select-Object Name, LastWriteTime

# 6. 驗證 GitHub Release
# 訪問: https://github.com/TwIcePenguin/nobapp/releases
```

## 故障排除決策樹

```
發佈執行完成後
│
├─ 看到 "🚀 開始執行自訂發佈後置工作" 消息？
│  ├─ 否 → CustomActionsAfterPublish 未觸發
│  │      檢查: .csproj 中是否有該目標
│  │    檢查: Configuration 是否為 Release
│  │      檢查: PublishDir 是否正確
│  │
│  └─ 是 → 繼續...
│
├─ VersionInfo.cs 版本號增加了嗎？
│  ├─ 否 → UpdateVersion.ps1 失敗
│  │   檢查: PowerShell 執行策略
│  │      檢查: VersionInfo.cs 是否存在
│  │  檢查: 版本號格式是否正確 (X.Y.Z)
│  │
│  └─ 是 → 繼續...
│
├─ C:\BOT\ 有新的 v*.zip 文件嗎？
│  ├─ 否 → 7-Zip 或打包失敗
│  │      檢查: 7-Zip 是否安裝在 C:\Program Files\7-Zip\
│  │      檢查: C:\BOT\PS\ 中是否有文件
│  │      檢查: 磁盤空間是否充足
│  │
│  └─ 是 → 繼續...
│
└─ GitHub Release v*.* 已建立嗎？
   ├─ 否 → GitHub 上傳失敗
   │      檢查: GITHUB_TOKEN 是否設定
   │      檢查: Token 是否有效
   │      檢查: Token 是否有 repo 權限
   │      檢查: Git 是否可用
   │      檢查: Git 倉庫是否正確配置
   │
   └─ 是 → ✅ 發佈流程完成！
```

## 常見問題及解決方案

### Q: 如何驗證 GitHub Token 是否有效？
```powershell
$headers = @{"Authorization" = "token $env:GITHUB_TOKEN"}
Invoke-RestMethod -Uri "https://api.github.com/user" -Headers $headers
```

### Q: 如何手動測試 7-Zip？
```powershell
& "C:\Program Files\7-Zip\7z.exe" a -tzip "C:\BOT\test.zip" "C:\BOT\PS\*"
```

### Q: 如何查看 CustomActionsAfterPublish 是否執行？
- Visual Studio: 工具 → 選項 → 建置和執行 → MSBuild 輸出詳細程度設為"詳細"
- 命令行: 使用 `-v:diag` 參數查看診斷信息

### Q: 版本號格式有什麼要求？
- 格式: `X.Y.Z`（三個數字，用 . 分隔）
- 正確例子: `0.84.3`, `1.0.0`, `2.1.5`
- 錯誤例子: `0.84`, `1.0.0.0`, `v0.84.3`

## 下一步

1. ✅ 驗證所有必須安裝的工具都已安裝
2. ✅ 設定 GITHUB_TOKEN 環境變數
3. ✅ 執行第一次發佈測試
4. ✅ 驗證所有三個步驟都成功完成
5. ✅ 檢查 GitHub Release 是否正確建立

---

**最後更新**: 2025-01-23
**配置版本**: 1.0
**狀態**: ✅ 就緒，可以發佈
