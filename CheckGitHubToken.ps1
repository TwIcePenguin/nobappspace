# CheckGitHubToken.ps1 - 检查 GitHub Token 是否正确设置

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🔐 GitHub Token 檢查工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 檢查 1: 環境變數
Write-Host "1️⃣  檢查環境變數..." -ForegroundColor Yellow
$envToken = $env:GITHUB_TOKEN

if ([string]::IsNullOrEmpty($envToken)) {
    Write-Host "❌ \$env:GITHUB_TOKEN 未設置" -ForegroundColor Red
    Write-Host ""
} else {
    Write-Host "✅ \$env:GITHUB_TOKEN 已設置" -ForegroundColor Green
    
    # 顯示 Token 的部分內容（安全起見只顯示前幾個字符）
    $tokenPreview = $envToken.Substring(0, [math]::Min(10, $envToken.Length)) + "..."
    Write-Host "   Token 預覽: $tokenPreview" -ForegroundColor Green
    
    # 驗證 Token 格式
    if ($envToken -match "^ghp_") {
  Write-Host "   ✅ Token 格式正確 (Personal Access Token)" -ForegroundColor Green
    } elseif ($envToken -match "^gho_") {
        Write-Host "   ✅ Token 格式正確 (OAuth Token)" -ForegroundColor Green
    } elseif ($envToken -match "^ghu_") {
        Write-Host "✅ Token 格式正確 (User-to-Server Token)" -ForegroundColor Green
    } else {
        Write-Host " ⚠️  Token 格式可能不正確" -ForegroundColor Yellow
    }
    
    Write-Host "   長度: $($envToken.Length) 字符" -ForegroundColor Gray
    Write-Host ""
}

# 檢查 2: Git 配置
Write-Host "2️⃣  檢查 Git 配置..." -ForegroundColor Yellow
$gitName = git config --global user.name
$gitEmail = git config --global user.email

if ([string]::IsNullOrEmpty($gitName)) {
    Write-Host "❌ Git 用戶名未設置" -ForegroundColor Red
} else {
    Write-Host "✅ Git 用戶名: $gitName" -ForegroundColor Green
}

if ([string]::IsNullOrEmpty($gitEmail)) {
    Write-Host "❌ Git 郵箱未設置" -ForegroundColor Red
} else {
    Write-Host "✅ Git 郵箱: $gitEmail" -ForegroundColor Green
}

Write-Host ""

# 檢查 3: 測試連接（可選）
Write-Host "3️⃣  測試 GitHub API 連接..." -ForegroundColor Yellow

if (-not [string]::IsNullOrEmpty($envToken)) {
    try {
        $headers = @{
            "Authorization" = "token $envToken"
   "Accept" = "application/vnd.github.v3+json"
            "X-GitHub-Api-Version" = "2022-11-28"
        }
  
        # 測試連接到 GitHub API
    $response = Invoke-RestMethod -Uri "https://api.github.com/user" -Headers $headers -ErrorAction Stop
        
        Write-Host "✅ GitHub 連接成功！" -ForegroundColor Green
      Write-Host "   用戶: $($response.login)" -ForegroundColor Green
        Write-Host "   公開倉庫: $($response.public_repos)" -ForegroundColor Green
        
    } catch {
        Write-Host "❌ GitHub 連接失敗：$($_.Exception.Message)" -ForegroundColor Red
        Write-Host "   可能是 Token 無效或網路問題" -ForegroundColor Yellow
    }
} else {
    Write-Host "⏭️  跳過 (Token 未設置)" -ForegroundColor Gray
}

Write-Host ""

# 檢查 4: 存儲庫訪問權限
Write-Host "4️⃣  檢查倉庫訪問權限..." -ForegroundColor Yellow

if (-not [string]::IsNullOrEmpty($envToken)) {
    try {
        $repoUrl = "https://api.github.com/repos/TwIcePenguin/nobapp"
    $response = Invoke-RestMethod -Uri $repoUrl -Headers $headers -ErrorAction Stop
        
    Write-Host "✅ 可以訪問 TwIcePenguin/nobapp 倉庫" -ForegroundColor Green
        Write-Host "   描述: $($response.description)" -ForegroundColor Green
        Write-Host "   Star: $($response.stargazers_count)" -ForegroundColor Green
        
    } catch {
        Write-Host "❌ 無法訪問倉庫：$($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "⏭️  跳過 (Token 未設置)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

# 最終建議
if ([string]::IsNullOrEmpty($envToken)) {
    Write-Host ""
    Write-Host "⚠️  需要設置 GitHub Token:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "方式 1: 臨時設置（本次會話有效）" -ForegroundColor Gray
 Write-Host "  `$env:GITHUB_TOKEN = 'ghp_你的Token'" -ForegroundColor Gray
    Write-Host ""
    Write-Host "方式 2: 永久設置（Windows 環境變數）" -ForegroundColor Gray
    Write-Host "  Win + X > 系統 > 進階系統設定 > 環境變數" -ForegroundColor Gray
    Write-Host "  新增用戶變量：" -ForegroundColor Gray
    Write-Host "    GITHUB_TOKEN = ghp_你的Token" -ForegroundColor Gray
    Write-Host ""
    Write-Host "方式 3: 生成新 Token" -ForegroundColor Gray
    Write-Host "  訪問: https://github.com/settings/tokens/new" -ForegroundColor Gray
    Write-Host "  範圍選擇: repo (完整倉庫控制)" -ForegroundColor Gray
    Write-Host ""
} else {
    Write-Host "✅ Token 已正確設置，可以進行發佈！" -ForegroundColor Green
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
