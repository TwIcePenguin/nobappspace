# UpdateVersion.ps1
$versionFile = "VersionInfo.cs"
$versionPattern = 'public const string Version = "(\d+)\.(\d+)\.(\d+)"'

if (Test-Path $versionFile) {
    $content = Get-Content $versionFile -Raw
    if ($content -match $versionPattern) {
        $majorVersion = $matches[1]
        $minorVersion = $matches[2]
        $buildNumber = [int]$matches[3] + 1
        $newVersion = "$majorVersion.$minorVersion.$buildNumber"
        $newContent = $content -replace $versionPattern, "public const string Version = `"$newVersion`""
        Set-Content $versionFile -Value $newContent
        Write-Host "更新版本至 $newVersion"
    } else {
        Write-Host "在 $versionFile 中找不到版本模式"
    }
} else {
    Write-Host "找不到 $versionFile 檔案"
}
