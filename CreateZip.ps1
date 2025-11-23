# CreateZip.ps1 - 建立 ZIP 檔案
param (
 [string]$SourcePath = "",
 [string]$ZipPath = ""
)
$ErrorActionPreference = 'Stop'
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "📦 ZIP 檔案建立腳本 (win-x86 專用)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 正規化路徑
$SourcePath = ($SourcePath).Trim()
$ZipPath = ($ZipPath).Trim()

if ([string]::IsNullOrEmpty($SourcePath)) { Write-Host "❌ 未提供來源路徑" -ForegroundColor Red; exit1 }
if ([string]::IsNullOrEmpty($ZipPath)) { Write-Host "❌ 未提供 ZIP 路徑" -ForegroundColor Red; exit1 }
if (-not (Test-Path $SourcePath)) { Write-Host "❌來源路徑不存在: $SourcePath" -ForegroundColor Red; exit1 }

#只允許打包 win-x86目錄 (防止誤包 publish 上層)
$leaf = Split-Path $SourcePath -Leaf
if ($leaf -ne 'win-x86') {
 Write-Host "⚠️ 警告:來源目錄並非 win-x86，已取消打包。來源: $leaf" -ForegroundColor Yellow
 exit1
}

# 確保壓縮目標資料夾存在
$targetDir = [System.IO.Path]::GetDirectoryName($ZipPath)
if (-not (Test-Path $targetDir)) { New-Item -ItemType Directory -Path $targetDir | Out-Null }

Write-Host "📁來源: $SourcePath" -ForegroundColor Gray
Write-Host "📦目標: $ZipPath" -ForegroundColor Gray

# 收集要打包的檔案 (僅 win-x86 下內容)
$allFiles = Get-ChildItem -Path $SourcePath -Recurse -File
if ($allFiles.Count -eq0) { Write-Host "⚠️ win-x86目錄沒有檔案" -ForegroundColor Yellow; exit1 }

# 建立暫存 staging 資料夾避免鎖定
$stagingDir = Join-Path ([System.IO.Path]::GetTempPath()) ("nobapp_staging_" + [Guid]::NewGuid().ToString())
New-Item -ItemType Directory -Path $stagingDir | Out-Null

Write-Host "🧪 暫存 staging: $stagingDir" -ForegroundColor Gray

# 複製檔案 (保留相對路徑結構)
foreach ($f in $allFiles) {
 $rel = $f.FullName.Substring($SourcePath.Length).TrimStart('\','/')
 $destFile = Join-Path $stagingDir $rel
 $destDir = [System.IO.Path]::GetDirectoryName($destFile)
 if (-not (Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir | Out-Null }
 try { Copy-Item -Path $f.FullName -Destination $destFile -Force } catch { Write-Host "⚠️ 複製失敗: $($f.FullName) -> $($_.Exception.Message)" -ForegroundColor Yellow }
}

# 載入壓縮組件
try { Add-Type -AssemblyName System.IO.Compression.FileSystem } catch { Write-Host "❌ 無法載入壓縮組件: $($_.Exception.Message)" -ForegroundColor Red; Remove-Item $stagingDir -Recurse -Force; exit1 }

# 若舊 ZIP 存在，先刪除
if (Test-Path $ZipPath) {
 try { Remove-Item $ZipPath -Force } catch { Write-Host "⚠️ 無法刪除舊 ZIP: $($_.Exception.Message)" -ForegroundColor Yellow }
}

# 建立新的 ZIP
try {
 Write-Host "⬆️ 建立 ZIP (僅 win-x86內容)..." -ForegroundColor Cyan
 [System.IO.Compression.ZipFile]::CreateFromDirectory($stagingDir, $ZipPath, [System.IO.Compression.CompressionLevel]::Optimal, $false)
 Write-Host "✅ ZIP 建立完成" -ForegroundColor Green
} catch {
 Write-Host "❌ 壓縮失敗: $($_.Exception.Message)" -ForegroundColor Red
 try { if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force } } catch {}
 Remove-Item $stagingDir -Recurse -Force
 exit1
}

# 清理 staging
try { Remove-Item $stagingDir -Recurse -Force } catch { Write-Host "⚠️ 暫存清理失敗: $($_.Exception.Message)" -ForegroundColor Yellow }

if (Test-Path $ZipPath) {
 $zipSizeMB = [math]::Round((Get-Item $ZipPath).Length /1MB,2)
 Write-Host "📊 大小: $zipSizeMB MB" -ForegroundColor Green
 Write-Host "📍 路徑: $ZipPath" -ForegroundColor Green
 exit0
} else { Write-Host "❌ 最終 ZIP 不存在" -ForegroundColor Red; exit1 }
