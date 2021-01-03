$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$solution = Join-Path $PSScriptRoot ".." "GRXoft.Extensions.sln"
$results = Join-Path $PSScriptRoot "Results" "$timestamp"
$resultsXml = Join-Path $results "**" "coverage.cobertura.xml"
$reports = Join-Path $PSScriptRoot "Reports"
$reportsHtml = Join-Path $reports "index.html"
$location = Get-Location

Set-Location $PSScriptRoot

dotnet tool restore
dotnet test $solution --collect "XPlat Code Coverage" --results-directory $results
dotnet reportgenerator -reports:$resultsXml -targetdir:$reports

Set-Location $location

& $reportsHtml -ErrorAction Continue

Remove-Item -Path $results -Force -Recurse
