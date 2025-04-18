param(
    [string]$SettingsFilePath = "../config/local-build-settings.json"
)

if ((Test-Path $SettingsFilePath)) {
    Write-Host "✅ Settings file: $SettingsFilePath found"    
}
else {
    Write-Host "❌ Settings file: $SettingsFilePath not found"
    exit 1
}

$Settings = Get-Content $SettingsFilePath -Raw | ConvertFrom-Json

$AbsoluteSolutionPath = (Resolve-Path -Path $Settings.SolutionRelativePath).Path
$AbsoluteOutputPath   = (Resolve-Path -Path $Settings.OutputPath).Path

$SolutionPath  = $Settings.SolutionRelativePath
$OutputPath    = $Settings.OutputPath
$Configuration = $Settings.Configuration

Write-Host "🧪 Absolute Solution path: $AbsoluteSolutionPath"

Write-Host "🧪 Absolute Output path: $AbsoluteOutputPath"

Write-Host "⭐ [1/5] Restoring solution packages $Settings.SolutionRelativePath ..."
dotnet restore $Settings.SolutionRelativePath

Write-Host "⭐ [2/5] Building solution $SolutionPath ..."
dotnet build $SolutionPath --no-restore --configuration $Configuration

$CoverageResultDir = Join-Path $PSScriptRoot "../tests-results"
Write-Host "⭐ [3/5] Running tests ..."
dotnet test $SolutionPath --no-build --configuration $Configuration --verbosity detailed --collect:"XPlat Code Coverage" --results-directory:"$CoverageResultDir" -- RunConfiguration.FailFast=true

$CoverageReportDir = Join-Path $PSScriptRoot "../tests-results/coverage-report"
Write-Host "⭐ [4/5] Saving tests report ..."
reportgenerator -reports:"$CoverageResultDir/**/coverage.cobertura.xml" -targetdir:"$CoverageReportDir" -reporttypes:Cobertura,Html

Write-Host "⭐ [5/5] Packing to $AbsoluteOutputPath ..."
dotnet pack $SolutionPath --no-build --configuration $Configuration --output $OutputPath