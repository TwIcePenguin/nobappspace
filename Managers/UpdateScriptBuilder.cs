using System;
using System.IO;

namespace NOBApp.Managers
{
    public static class UpdateScriptBuilder
    {
        public static string Build()
        {
            return @"# UTF-8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Write-Host '===企鵝之野望 更新程序 ===' -ForegroundColor Cyan
$logFile = 'update_log.txt'
'['+(Get-Date)+'] 開始更新' | Out-File -FilePath $logFile -Encoding utf8
if (-not (Test-Path 'update.zip')) { Write-Host '找不到 update.zip' -ForegroundColor Red; exit1 }
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead('update.zip')
foreach ($entry in $zip.Entries) {
 $targetPath = [IO.Path]::Combine('.', $entry.FullName)
 $dir = [IO.Path]::GetDirectoryName($targetPath)
 if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }
 if (-not $entry.FullName.EndsWith('/')) {
 if (Test-Path $targetPath) { Remove-Item $targetPath -Force -ErrorAction SilentlyContinue }
 $entryStream = $entry.Open(); $targetStream = [IO.File]::Create($targetPath); $entryStream.CopyTo($targetStream); $targetStream.Close(); $entryStream.Close();
 }
}
$zip.Dispose()
Remove-Item 'update.zip' -Force -ErrorAction SilentlyContinue
'更新成功於 '+(Get-Date) | Out-File -FilePath 'update_success.txt' -Encoding utf8
Start-Process 'NOBApp.exe' -ErrorAction SilentlyContinue
";
        }
    }
}
