$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

$files = Get-ChildItem -Path $repoRoot -Recurse -File |
  Where-Object {
    $_.FullName -notmatch '\\(bin|obj|\.git|node_modules)\\' -and
    $_.Extension -in '.cs','.cshtml','.razor','.js','.ts','.css','.json','.xml','.config','.sln','.csproj'
  }

$lineEndings = foreach ($file in $files) {
  $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
  $text = [System.Text.Encoding]::UTF8.GetString($bytes)

  $crlf = ([regex]::Matches($text, "`r`n")).Count
  $lfOnly = ([regex]::Matches($text, "(?<!`r)`n")).Count

  [pscustomobject]@{
    File = $file.FullName.Substring($repoRoot.Path.Length + 1)
    CRLF = $crlf
    LFOnly = $lfOnly
    Type = if ($crlf -gt 0 -and $lfOnly -eq 0) { "CRLF" }
           elseif ($lfOnly -gt 0 -and $crlf -eq 0) { "LF" }
           elseif ($crlf -gt 0 -and $lfOnly -gt 0) { "Mixed" }
           else { "No line endings" }
  }
}

Write-Host "`nSummary:"
$lineEndings | Group-Object Type | Select-Object Name, Count | Format-Table -AutoSize

Write-Host "`nFiles with LF:"
$lineEndings |
  Where-Object Type -eq "LF" |
  Select-Object File, CRLF, LFOnly, Type |
  Format-Table -AutoSize

Write-Host "`nFiles with Mixed line endings:"
$lineEndings |
  Where-Object Type -eq "Mixed" |
  Select-Object File, CRLF, LFOnly, Type |
  Format-Table -AutoSize