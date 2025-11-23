 param (
       [string]$FilePath,
       [int]$MaxRetries = 5,
       [int]$DelaySeconds = 2
   )

   $retry = 0
   while ($retry -lt $MaxRetries) {
       try {
           $stream = [System.IO.File]::Open($FilePath, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::Read)
           $stream.Close()
           break
       } catch {
           Start-Sleep -Seconds $DelaySeconds
           $retry++
       }
   }

   if ($retry -eq $MaxRetries) {
       Write-Error "�ɮ� $FilePath ���Q��w�A�L�k�~�����Y�C"
       exit 1
   }
   