# 📋 完整發佈流程 - 技術實現總結

## 系統架構

```
NOBApp.csproj (發佈觸發器)
    ↓
[CustomActionsAfterPublish Target]
    ├─ UpdateVersion.ps1 (版本管理)
 │   └─ 讀取 VersionInfo.cs
    │   └─ 自動遞增版本號
    │   └─ 寫入 VersionInfo.cs
    │
    ├─ CreateZip.ps1 (應用打包)
    │   └─ 驗證發佈目錄
  │   └─ 刪除舊 ZIP 文件
    │   └─ 使用 Compress-Archive 創建 ZIP
    │ └─ 驗證 ZIP 文件完整性
    │
    └─ PostBuildScript.ps1 (GitHub 發佈)
     ├─ Git 版本提交
  ├─ Git 推送到 main 分支
        ├─ 創建 GitHub Release
        ├─ 上傳 ZIP 到 Release
        └─ 生成發佈 URL
```

---

## 關鍵文件說明

### 1. `NOBApp.csproj`
**作用**：發佈工作流程的協調者

```xml
<Target Name="CustomActionsAfterPublish" AfterTargets="Publish">
    <!-- 依序執行三個 PowerShell 腳本 -->
 <Exec Command="... UpdateVersion.ps1 ..." />
    <Exec Command="... CreateZip.ps1 ..." />
 <Exec Command="... PostBuildScript.ps1 ..." />
</Target>
```

**重要屬性**：
- `AfterTargets="Publish"` - 在發佈完成後執行
- `Condition="'$(PublishDir)' != ''"` - 確保發佈目錄存在

---

### 2. `UpdateVersion.ps1`
**作用**：版本號自動管理

**功能**：
1. 讀取 VersionInfo.cs
2. 提取當前版本號
3. 遞增版本號（最後一位）
4. 寫回 VersionInfo.cs

**示例**：
```csharp
// 修改前
public const string Version = "0.84.7";

// 修改後
public const string Version = "0.84.8";
```

---

### 3. `CreateZip.ps1` ⭐ (新增)
**作用**：可靠的 ZIP 文件創建

**參數**：
```powershell
-SourcePath  : 發佈目錄路徑
-ZipPath     : ZIP 文件輸出路徑
```

**工作流程**：
```powershell
1. 驗證參數和路徑
2. 刪除舊的 ZIP 文件
3. 使用 Compress-Archive 創建新 ZIP
4. 驗證 ZIP 文件是否成功創建
5. 報告 ZIP 文件大小
```

**關鍵改進**：
- ✅ 避免 MSBuild 轉義問題
- ✅ 完整的錯誤處理
- ✅ 詳細的日誌輸出
- ✅ 退出代碼正確設置

---

### 4. `PostBuildScript.ps1`
**作用**：GitHub Release 自動發佈

**四個步驟**：

#### 步驟 1：版本提交
```powershell
git add VersionInfo.cs
git commit -m "chore: Release v0.84.8"
```

#### 步驟 2：推送到 GitHub
```powershell
git push origin main
```

#### 步驟 3：建立 GitHub Release
```powershell
POST https://api.github.com/repos/TwIcePenguin/nobapp/releases
{
    "tag_name": "v0.84.8",
    "name": "Release v0.84.8",
    "draft": false,
    "prerelease": false
}
```

#### 步驟 4：上傳 ZIP 到 Release
```powershell
POST https://uploads.github.com/repos/TwIcePenguin/nobapp/releases/.../assets
Content-Type: application/zip
[Binary ZIP Content]
```

---

## 錯誤處理機制

### 1. 參數驗證
```powershell
if ([string]::IsNullOrEmpty($OutputPath)) {
    Write-Host "❌ 未提供輸出路徑"
    exit 0  # 非致命錯誤，繼續執行
}
```

### 2. 路徑檢查
```powershell
if (-not (Test-Path $VersionInfoPath)) {
    Write-Host "❌ 找不到版本文件"
  exit 0
}
```

### 3. 步驟條件執行
```powershell
if ($LASTEXITCODE -eq 0) {
    # 前一步成功，執行下一步
} else {
 # 前一步失敗，跳過或報錯
}
```

### 4. Try-Catch 異常捕獲
```powershell
try {
    Compress-Archive -Path "$SourcePath*" -DestinationPath $ZipPath
}
catch {
    Write-Host "❌ 錯誤: $($_.Exception.Message)"
    exit 1  # 致命錯誤，停止執行
}
```

---

## 版本管理策略

### 版本號格式
```
0.84.8
│ │  │
│ │  └─ 補丁版本（自動遞增）
│ └──── 次版本
└────── 主版本
```

### 自動遞增規則
```powershell
[version]$currentVersion = "0.84.7"
$newVersion = [version]::new(
    $currentVersion.Major,
    $currentVersion.Minor,
    $currentVersion.Build + 1
)  # 結果：0.84.8
```

---

## GitHub API 集成

### 認證
```powershell
$headers = @{
    "Authorization" = "token $GitHubToken"
    "Accept" = "application/vnd.github.v3+json"
}
```

### Release 生命週期

```
1. 檢查 Release 是否存在
   GET /repos/{owner}/{repo}/releases/tags/v0.84.8

2. 如果存在，更新 Release
   PATCH /repos/{owner}/{repo}/releases/{id}

3. 如果不存在，建立新 Release
   POST /repos/{owner}/{repo}/releases

4. 上傳 ZIP 資源
   POST /repos/{owner}/{repo}/releases/{id}/assets
```

### Release 描述範本
```markdown
## Automated Release

**Version**: v0.84.8
**Built**: 2025-11-23 04:39:08

[Download v0.84.8.zip](https://github.com/TwIcePenguin/nobapp/releases/download/v0.84.8/v0.84.8.zip)
```

---

## 安全考量

### GitHub Token 安全
```powershell
# ✅ 推薦：使用環境變量
$GitHubToken = $env:GITHUB_TOKEN

# ❌ 不推薦：硬編碼 token
$GitHubToken = "ghp_xxxxxxxxxxxx"
```

### 日誌中隱藏敏感信息
```powershell
Write-Host "GitHubToken = $(if ($GitHubToken) { '(set)' } else { '(empty)' })"
```

---

## 性能優化

### ZIP 壓縮級別
```powershell
Compress-Archive -CompressionLevel Optimal
# 最高壓縮率，適合發佈文件
```

### 異步操作
```powershell
# 不涉及異步，所有操作順序執行
# 確保每個步驟成功才進行下一步
```

---

## 監控與日誌

### 輸出等級
```powershell
Write-Host "..." -ForegroundColor Cyan      # 標題
Write-Host "..." -ForegroundColor Green     # 成功
Write-Host "..." -ForegroundColor Yellow    # 警告
Write-Host "..." -ForegroundColor Red   # 錯誤
```

### 日誌分類
```
📤 過程標記
✅ 成功標記
⚠️  警告標記
❌ 錯誤標記
```

---

## 故障排查

### 常見問題

#### 問題 1：GITHUB_TOKEN 未設置
```powershell
# 解決方案
$env:GITHUB_TOKEN = "your_token_here"
```

#### 問題 2：Git 配置錯誤
```powershell
# 檢查 Git 配置
git config --global user.name
git config --global user.email

# 設置配置
git config --global user.name "Your Name"
git config --global user.email "your@email.com"
```

#### 問題 3：ZIP 創建失敗
```powershell
# 檢查發佈目錄
Get-ChildItem "bin\Release\net8.0-windows7.0\publish\win-x86\" | Measure-Object

# 檢查磁盤空間
Get-Volume C:
```

---

## 部署檢查清單

- [ ] Visual Studio 已安裝最新版本
- [ ] .NET 8 SDK 已安裝
- [ ] Git 已安裝並配置
- [ ] GitHub Token 已設置
- [ ] PowerShell 執行策略允許 Bypass
- [ ] 發佈目錄有寫入權限
- [ ] 網路連接正常
- [ ] GitHub 倉庫可訪問

---

## 參考資源

- [Microsoft Learn: MSBuild Targets](https://learn.microsoft.com/en-us/visualstudio/msbuild/)
- [PowerShell: Compress-Archive](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.archive/)
- [GitHub: REST API](https://docs.github.com/en/rest/)
- [.NET: Publishing](https://learn.microsoft.com/en-us/dotnet/core/deploying/)

---

**文檔版本**：1.0  
**最後更新**：2025-11-23  
**狀態**：✅ 完整

