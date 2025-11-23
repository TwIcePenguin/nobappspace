# CheckForTokens.ps1 - 检查代码中是否有硬编码的 GitHub Token

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🔍 检查硬编码 Token" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 要搜索的 Token 模式
$tokenPatterns = @(
    "ghp_",  # GitHub Personal Access Token
    "gho_",  # GitHub OAuth Token
    "ghu_",  # GitHub User-to-Server Token
  "ghs_",  # GitHub Server-to-Server Token
    "ghr_"# GitHub Refresh Token
)

# 要搜索的文件类型
$fileExtensions = @("*.ps1", "*.cs", "*.xml", "*.csproj", "*.xaml")

# 要排除的目录
$excludeDirs = @("bin", "obj", ".git", "node_modules", ".vs")

Write-Host "📋 搜索参数："
Write-Host "Token 模式: $($tokenPatterns -join ', ')"
Write-Host "   文件类型: $($fileExtensions -join ', ')"
Write-Host "   排除目录: $($excludeDirs -join ', ')"
Write-Host ""

$foundTokens = @()

# 搜索所有文件
foreach ($extension in $fileExtensions) {
    Write-Host "🔍 搜索 $extension 文件..." -ForegroundColor Yellow
    
    $files = Get-ChildItem -Path . -Recurse -Include $extension -ErrorAction SilentlyContinue
    
    foreach ($file in $files) {
     # 检查是否在排除目录中
  $shouldExclude = $false
    foreach ($excludeDir in $excludeDirs) {
          if ($file.FullName -like "*\$excludeDir\*") {
    $shouldExclude = $true
      break
    }
      }
  
      if ($shouldExclude) {
   continue
  }
        
        # 搜索 Token
        $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
        
        foreach ($pattern in $tokenPatterns) {
    if ($content -like "*$pattern*") {
                $foundTokens += @{
    File = $file.FullName
 Pattern = $pattern
     }
      
   Write-Host "   ❌ 找到 Token: $($file.FullName)" -ForegroundColor Red
          }
      }
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

if ($foundTokens.Count -eq 0) {
    Write-Host "✅ 未找到任何硬编码的 Token" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "🚨 找到 $($foundTokens.Count) 個硬编码 Token！" -ForegroundColor Red
    Write-Host ""
    
    foreach ($token in $foundTokens) {
        Write-Host "📁 $($token.File)" -ForegroundColor Red
     Write-Host "   模式: $($token.Pattern)" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "⚠️  需要立即移除这些 Token！" -ForegroundColor Red
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
