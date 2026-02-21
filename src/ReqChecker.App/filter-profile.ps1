<#
.SYNOPSIS
    Filters default-profile.json to only include test entries matching the specified types.

.DESCRIPTION
    Called by the FilterDefaultProfile MSBuild target in ReqChecker.App.csproj during
    conditional customer builds. Removes test entries whose 'type' field is not in the
    IncludeTests list, and writes the filtered JSON to the specified output path.

.PARAMETER SourcePath
    Path to the source default-profile.json file.

.PARAMETER OutputPath
    Path where the filtered JSON file will be written.

.PARAMETER IncludeTests
    Semicolon-separated list of test type identifiers to keep (e.g., "Ping;HttpGet").
#>

param(
    [Parameter(Mandatory)] [string] $SourcePath,
    [Parameter(Mandatory)] [string] $OutputPath,
    [Parameter(Mandatory)] [string] $IncludeTests
)

$types = $IncludeTests -split ';' | Where-Object { $_ -ne '' }

$json = Get-Content -Raw -Path $SourcePath | ConvertFrom-Json

$originalCount = $json.tests.Count

$json.tests = @($json.tests | Where-Object { $_.type -in $types })

# Strip dependsOn entries that reference tests removed by type filtering
$keptIds = $json.tests | Select-Object -ExpandProperty id
$strippedCount = 0
foreach ($test in $json.tests) {
    $before = @($test.dependsOn).Count
    $test.dependsOn = @($test.dependsOn | Where-Object { $_ -in $keptIds })
    $strippedCount += $before - @($test.dependsOn).Count
}
if ($strippedCount -gt 0) {
    Write-Host "FilterDefaultProfile: stripped $strippedCount dangling dependsOn reference(s)"
}

$filteredCount = $json.tests.Count

# Ensure output directory exists
$outputDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

$json | ConvertTo-Json -Depth 20 | Set-Content -Path $OutputPath -Encoding UTF8

Write-Host "FilterDefaultProfile: kept $filteredCount of $originalCount test entries for types: $($types -join ', ')"
