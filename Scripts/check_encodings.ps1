$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

$files = Get-ChildItem -Path $repoRoot -Recurse -File |
  Where-Object {
    $_.FullName -notmatch '\\(bin|obj|\.git|node_modules)\\' -and
    $_.Extension -in '.cs','.cshtml','.razor','.js','.ts','.css','.json','.xml','.config','.sln','.csproj'
  }

$utf8Strict = [System.Text.UTF8Encoding]::new($false, $true)

$encodingCheck = foreach ($file in $files) {
  $bytes = [System.IO.File]::ReadAllBytes($file.FullName)

  try {
    $null = $utf8Strict.GetString($bytes)

    $encoding =
      if ($bytes.Length -ge 3 -and
          $bytes[0] -eq 0xEF -and
          $bytes[1] -eq 0xBB -and
          $bytes[2] -eq 0xBF) {
        "UTF-8 with BOM"
      }
      else {
        "UTF-8 without BOM"
      }
  }
  catch {
    $encoding = "Not valid UTF-8, likely Windows-1252/ANSI"
  }

  [pscustomobject]@{
    File = $file.FullName.Substring($repoRoot.Path.Length + 1)
    Encoding = $encoding
  }
}

Write-Host "`nSummary:"

$summary = @(
  [pscustomobject]@{
    Encoding = "UTF-8 with BOM"
    Count = ($encodingCheck | Where-Object Encoding -eq "UTF-8 with BOM").Count
  }
  [pscustomobject]@{
    Encoding = "UTF-8 without BOM"
    Count = ($encodingCheck | Where-Object Encoding -eq "UTF-8 without BOM").Count
  }
  [pscustomobject]@{
    Encoding = "Not valid UTF-8, likely Windows-1252/ANSI"
    Count = ($encodingCheck | Where-Object Encoding -eq "Not valid UTF-8, likely Windows-1252/ANSI").Count
  }
)

$summary | Format-Table -AutoSize

Write-Host "`nFiles that are not valid UTF-8:"
$encodingCheck |
  Where-Object Encoding -eq "Not valid UTF-8, likely Windows-1252/ANSI" |
  Select-Object File, Encoding |
  Format-Table -AutoSize
