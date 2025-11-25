Param(
    [string]$Root = "$(Resolve-Path .).Path",
    [string[]]$Extensions = @('*.cs','*.xaml','*.resx','*.config','*.json','*.xml','*.txt')
)

Write-Host "Scanning for files to convert under: $Root"

# Normalize root path
if (-not (Test-Path $Root)) {
    Write-Error "Root path not found: $Root"
    exit 1
}

$files = Get-ChildItem -Path $Root -Recurse -Include $Extensions -File -ErrorAction SilentlyContinue |
         Where-Object { -not ($_.FullName -like "*/.git/*") -and -not ($_.FullName -like "*\\.git\\*") }

if ($files -eq $null -or $files.Count -eq 0) {
    Write-Host "No files found for conversion."
    exit 0
}

# Encodings to try for detection (common for Chinese locales: 950=Big5, 936=GBK)
$encodingsToTry = @(
    [System.Text.Encoding]::UTF8,
    [System.Text.Encoding]::Unicode,            # UTF-16 LE
    [System.Text.Encoding]::BigEndianUnicode,  # UTF-16 BE
    [System.Text.Encoding]::GetEncoding(950),  # Big5
    [System.Text.Encoding]::GetEncoding(936),  # GBK
    [System.Text.Encoding]::GetEncoding(1252)  # ANSI Latin-1/Windows-1252
)

foreach ($f in $files) {
    try {
        $bytes = [System.IO.File]::ReadAllBytes($f.FullName)

        # Skip binary files heuristically (look for null bytes)
        if ($bytes -contains 0) {
            Write-Host "Skipping binary: $($f.FullName)"
            continue
        }

        # Detect existing UTF8 BOM
        if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
            Write-Host "Skipping (already UTF-8 BOM): $($f.FullName)"
            continue
        }

        $decoded = $null
        $detected = $null
        foreach ($enc in $encodingsToTry) {
            try {
                $s = $enc.GetString($bytes)
                # Re-encode and compare lengths and bytes
                $re = $enc.GetBytes($s)
                if ($re.Length -eq $bytes.Length) {
                    $same = $true
                    for ($i=0; $i -lt $bytes.Length; $i++) { if ($bytes[$i] -ne $re[$i]) { $same = $false; break } }
                    if ($same) { $decoded = $s; $detected = $enc; break }
                }
            } catch { }
        }

        if ($decoded -eq $null) {
            # Last resort: try to decode as UTF8 without BOM
            try {
                $utf8NoBom = [System.Text.Encoding]::GetEncoding('utf-8')
                $s = $utf8NoBom.GetString($bytes)
                $decoded = $s
                $detected = $utf8NoBom
            } catch {
                # fallback to system ANSI
                $fallback = [System.Text.Encoding]::Default
                $decoded = $fallback.GetString($bytes)
                $detected = $fallback
            }
        }

        # Write using UTF8 with BOM to make editors detect correct encoding
        $utf8Bom = New-Object System.Text.UTF8Encoding $true
        [System.IO.File]::WriteAllText($f.FullName, $decoded, $utf8Bom)
        Write-Host "Converted: $($f.FullName) from $($detected.WebName) to UTF-8 with BOM"
    } catch {
        Write-Warning "Failed to convert $($f.FullName): $_"
    }
}

Write-Host "Conversion complete. Consider running: git add --renormalize . && git commit -m 'Normalize file encodings to UTF-8 (BOM)'"
