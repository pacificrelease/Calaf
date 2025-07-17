param(
    [string]$SettingsFilePath = "../config/local-build-settings.json",
    [string]$CoverageResultDirectoryPath = "../tests/coverage-results",
    [string]$CoverageReportDirectoryPath = "../tests/coverage-reports"
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

#Test-Path -Path $directoryPath -PathType Container
if (Test-Path -Path  $CoverageResultDirectoryPath -PathType Container) {
    Write-Host "✅ Coverage result directory: $CoverageResultDirectoryPath found"
    Write-Host "🛠️ Cleaning $CoverageResultDirectoryPath"
    Get-ChildItem -Path $CoverageResultDirectoryPath -Recurse | Remove-Item -Force -Recurse
}
else {
    Write-Host "🧪 Coverage result directory: $CoverageResultDirectoryPath not found. Creating it now..."
    New-Item -ItemType Directory -Path $CoverageResultDirectoryPath
}

if (Test-Path -Path $CoverageReportDirectoryPath -PathType Container) {
    Write-Host "✅ Coverage report directory: $CoverageReportDirectoryPath found"
    Write-Host "🛠️ Cleaning $CoverageReportDirectoryPath"
    Get-ChildItem -Path $CoverageReportDirectoryPath -Recurse | Remove-Item -Force -Recurse
}
else {
    Write-Host "🧪 Coverage report directory: $CoverageReportDirectoryPath not found. Creating it now..."
    New-Item -ItemType Directory -Path $CoverageReportDirectoryPath
}

Write-Host "🧪 Absolute Solution path is: $AbsoluteSolutionPath"

Write-Host "🧪 Absolute Output path is: $AbsoluteOutputPath"

Write-Host "⭐ [1/5] Restoring solution packages $SolutionPath ..."
dotnet restore $SolutionPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Package restoration failed"
    exit $LASTEXITCODE
}

Write-Host "⭐ [2/5] Building solution $SolutionPath ..."
dotnet build $SolutionPath --no-restore --configuration $Configuration --verbosity detailed
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Solution building failed"
    exit $LASTEXITCODE
}

Write-Host "⭐ [3/5] Running tests ..."
dotnet test $SolutionPath --no-build --configuration $Configuration --verbosity detailed --collect:"XPlat Code Coverage" --results-directory:"$CoverageResultDirectoryPath" -- RunConfiguration.FailFast=true
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Running tests failed"
    exit $LASTEXITCODE
}

Write-Host "⭐ [4/5] Saving tests reports ..."
reportgenerator -reports:"$CoverageResultDirectoryPath/**/coverage.cobertura.xml" -targetdir:"$CoverageReportDirectoryPath" -reporttypes:Cobertura,Html
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Tests report did not saved"
    exit $LASTEXITCODE
}

Write-Host "⭐ [5/5] Packing to $AbsoluteOutputPath ..."
dotnet pack $SolutionPath --no-build --configuration $Configuration --output $OutputPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Pack artefact failed"
    exit $LASTEXITCODE
}