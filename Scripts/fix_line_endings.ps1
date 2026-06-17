$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

$extensions = @(
  '.cs',
  '.cshtml',
  '.razor',
  '.js',
  '.ts',
  '.css',
  '.json',
  '.xml',
  '.config',
  '.sln',
  '.csproj'
)

$files = Get-ChildItem -Path $repoRoot -Recurse -File |
  Where-Object {
    $_.FullName -notmatch '\\(bin|obj|\.git|node_modules)\\' -and
    $_.Extension -in $extensions
  }

$fixedFiles = foreach ($file in $files) {
  $bytes = [System.IO.File]::ReadAllBytes($file.FullName)

  # Detect UTF-8 BOM
  $hasBom =
    $bytes.Length -ge 3 -and
    $bytes[0] -eq 0xEF -and
    $bytes[1] -eq 0xBB -and
    $bytes[2] -eq 0xBF

  $utf8 = [System.Text.Encoding]::UTF8
  $text = $utf8.GetString($bytes)

  $crlf = ([regex]::Matches($text, "`r`n")).Count
  $lfOnly = ([regex]::Matches($text, "(?<!`r)`n")).Count

  $type =
    if ($crlf -gt 0 -and $lfOnly -eq 0) { "CRLF" }
    elseif ($lfOnly -gt 0 -and $crlf -eq 0) { "LF" }
    elseif ($crlf -gt 0 -and $lfOnly -gt 0) { "Mixed" }
    else { "No line endings" }

  if ($type -eq "Mixed") {
    # Normalize all endings to LF first, then convert LF to CRLF
    $normalized = $text -replace "`r`n", "`n"
    $normalized = $normalized -replace "`r", "`n"
    $crlfText = $normalized -replace "`n", "`r`n"

    # Preserve BOM if file had one
    $encoding = [System.Text.UTF8Encoding]::new($hasBom)

    [System.IO.File]::WriteAllText($file.FullName, $crlfText, $encoding)

    [pscustomobject]@{
      File = $file.FullName.Substring($repoRoot.Path.Length + 1)
      PreviousType = $type
      CRLF = $crlf
      LFOnly = $lfOnly
      FixedTo = "CRLF"
    }
  }
}

Write-Host "`nFixed files:"
if ($fixedFiles) {
  $fixedFiles | Format-Table -AutoSize
}
else {
  Write-Host "No files with Mixed line endings found."
}