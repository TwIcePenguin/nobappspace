# UpdateVersion.ps1
$versionFile = "VersionInfo.cs"
$versionPattern = 'public const string Version = "v0.(\d+).(\d+)"'

if (Test-Path $versionFile) {
    $content = Get-Content $versionFile -Raw
    if ($content -match $versionPattern) {
        $majorVersion = [int]$matches[1]
        $buildNumber = [int]$matches[2] + 1
        $newVersion = "v0.$majorVersion.$buildNumber"
        $newContent = $content -replace $versionPattern, "public const string Version = `"$newVersion`""
        Set-Content $versionFile -Value $newContent
        Write-Host "Updated version to $newVersion"
    } else {
        Write-Host "Version pattern not found in $versionFile"
    }
} else {
    Write-Host "$versionFile not found"
}