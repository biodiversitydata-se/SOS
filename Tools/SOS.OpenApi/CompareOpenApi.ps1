param(
    [Parameter(Mandatory)] [string] $BaselineDirectory,
    [Parameter(Mandatory)] [string] $CurrentDirectory
)

function Compare-JsonValue {
    param($Before, $After, [string] $Path, [System.Collections.Generic.List[object]] $Differences)

    if ($Before -is [System.Collections.IDictionary] -and $After -is [System.Collections.IDictionary]) {
        $keys = @($Before.Keys) + @($After.Keys) | Sort-Object -Unique
        foreach ($key in $keys) {
            $childPath = if ($Path) { "$Path.$key" } else { [string] $key }
            if (-not $Before.Contains($key) -or -not $After.Contains($key)) {
                $Differences.Add([pscustomobject]@{
                    Path = $childPath
                    Before = if ($Before.Contains($key)) { $Before[$key] | ConvertTo-Json -Compress -Depth 30 } else { '<absent>' }
                    After = if ($After.Contains($key)) { $After[$key] | ConvertTo-Json -Compress -Depth 30 } else { '<absent>' }
                })
            } else {
                Compare-JsonValue $Before[$key] $After[$key] $childPath $Differences
            }
        }
        return
    }

    if ($Before -is [array] -and $After -is [array]) {
        if ($Before.Count -ne $After.Count) {
            $Differences.Add([pscustomobject]@{ Path = "$Path.length"; Before = $Before.Count; After = $After.Count })
        }
        for ($index = 0; $index -lt [Math]::Min($Before.Count, $After.Count); $index++) {
            Compare-JsonValue $Before[$index] $After[$index] "$Path[$index]" $Differences
        }
        return
    }

    if (-not [object]::Equals($Before, $After)) {
        $Differences.Add([pscustomobject]@{ Path = $Path; Before = $Before; After = $After })
    }
}

foreach ($baselineFile in Get-ChildItem -LiteralPath $BaselineDirectory -Filter '*.json') {
    $currentFile = Join-Path $CurrentDirectory $baselineFile.Name
    if (-not (Test-Path -LiteralPath $currentFile)) { throw "Missing current OpenAPI document: $currentFile" }
    $before = Get-Content -LiteralPath $baselineFile.FullName -Raw | ConvertFrom-Json -AsHashtable
    $after = Get-Content -LiteralPath $currentFile -Raw | ConvertFrom-Json -AsHashtable
    $differences = [System.Collections.Generic.List[object]]::new()
    Compare-JsonValue $before $after '' $differences
    [pscustomobject]@{ Document = $baselineFile.BaseName; DifferenceCount = $differences.Count; Differences = $differences.ToArray() }
}
