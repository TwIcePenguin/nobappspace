# PostBuildScript.ps1
param (
    [string]$OutputPath,
    [string]$VersionInfoPath
)

$version = (Get-Content $VersionInfoPath -Raw | Select-String -Pattern 'public const string Version = "(.+)";' | ForEach-Object { $_.Matches.Groups[1].Value })
$zipFileName = "¥øÃZ¤§³¥±æv$version.zip"
$zipFilePath = Join-Path $OutputPath $zipFileName

Compress-Archive -Path "$OutputPath\*" -DestinationPath $zipFilePath
