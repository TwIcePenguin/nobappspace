# VersionSyncCheck.ps1 - 驗證版本號同步

param (
    [string]$ProjectDir = "H:\MemberSystem\nobappGitHub"
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🔍 版本號同步檢查" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. 檢查 VersionInfo.cs
Write-Host "步驟 1: 檢查 VersionInfo.cs"
$versionInfoFile = Join-Path $ProjectDir "VersionInfo.cs"
if (Test-Path $versionInfoFile) {
    $content = Get-Content $versionInfoFile -Raw
$match = $content | Select-String -Pattern 'public\s+const\s+string\s+Version\s*=\s*"([^"]+)"'
    if ($match) {
     $codeVersion = $match.Matches[0].Groups[1].Value
        Write-Host "✅ VersionInfo.cs 中的版本: $codeVersion" -ForegroundColor Green
    } else {
        Write-Host "❌ 無法從 VersionInfo.cs 中提取版本" -ForegroundColor Red
    }
} else {
    Write-Host "❌ 找不到 VersionInfo.cs" -ForegroundColor Red
}

# 2. 檢查 AssemblyInfo.cs
Write-Host ""
Write-Host "步驟 2: 檢查 AssemblyInfo.cs"
$assemblyInfoFile = Join-Path $ProjectDir "Properties\AssemblyInfo.cs"
if (Test-Path $assemblyInfoFile) {
    $content = Get-Content $assemblyInfoFile -Raw
    
    # 尋找 AssemblyVersion
    $versionMatch = $content | Select-String -Pattern 'AssemblyVersion\("([^"]+)"\)'
 if ($versionMatch) {
        $assemblyVersion = $versionMatch.Matches[0].Groups[1].Value
      Write-Host "✅ AssemblyVersion: $assemblyVersion" -ForegroundColor Green
    }
    
    # 尋找 AssemblyFileVersion
    $fileVersionMatch = $content | Select-String -Pattern 'AssemblyFileVersion\("([^"]+)"\)'
  if ($fileVersionMatch) {
        $assemblyFileVersion = $fileVersionMatch.Matches[0].Groups[1].Value
 Write-Host "✅ AssemblyFileVersion: $assemblyFileVersion" -ForegroundColor Green
    }
} else {
    Write-Host "⚠️  找不到 Properties\AssemblyInfo.cs (不是必須的)" -ForegroundColor Yellow
}

# 3. 檢查已發佈的執行檔
Write-Host ""
Write-Host "步驟 3: 檢查已發佈的執行檔"
$publishDir = Join-Path $ProjectDir "bin\Release\net8.0-windows7.0\publish\win-x86\"
$exeFile = Join-Path $publishDir "NOBApp.exe"

if (Test-Path $exeFile) {
    try {
 $exeVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($exeFile).FileVersion
   Write-Host "✅ 執行檔版本 (FileVersion): $exeVersion" -ForegroundColor Green
        
      $productVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($exeFile).ProductVersion
      Write-Host "✅ 執行檔版本 (ProductVersion): $productVersion" -ForegroundColor Green
    } catch {
        Write-Host "⚠️  無法讀取執行檔版本: $_" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠️  找不到發佈的執行檔: $exeFile" -ForegroundColor Yellow
    Write-Host " 請先執行: dotnet publish -c Release" -ForegroundColor Yellow
}

# 4. 版本比對
Write-Host ""
Write-Host "步驟 4: 版本號比對"
Write-Host ""

if ($codeVersion -and $exeVersion) {
 # 提取執行檔版本的主要部分（去掉 .0.0 部分）
    $exeVersionTrimmed = if ($exeVersion -match '^(\d+\.\d+\.\d+)') { $matches[1] } else { $exeVersion }
    
    if ($codeVersion -eq $exeVersionTrimmed) {
        Write-Host "✅ 版本號同步: $codeVersion" -ForegroundColor Green
    } else {
    Write-Host "⚠️  版本號不同步！" -ForegroundColor Red
    Write-Host "VersionInfo.cs: $codeVersion"
        Write-Host "   執行檔版本:     $exeVersion"
        Write-Host ""
    Write-Host "建議:"
        Write-Host "1. 確保 UpdateVersion.ps1 已正確更新 AssemblyInfo.cs"
        Write-Host "2. 重新發佈: dotnet publish -c Release"
    }
} else {
    Write-Host "⚠️  無法完成版本比對，缺少必要信息" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ 檢查完成" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
