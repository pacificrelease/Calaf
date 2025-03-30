param(
    [string]$SolutionPath = "../NCalVer.sln",
    [string]$Configuration = "Release",
    [string]$OutputPath = "D:/LocalNuGetFeed"
)

$AbsoluteSolutionPath = (Resolve-Path -Path $SolutionPath).Path
$AbsoluteOutputPath = (Resolve-Path -Path $OutputPath).Path

Write-Host "Absolute Solution path: $AbsoluteSolutionPath"

Write-Host "Absolute Output path: $AbsoluteOutputPath"

Write-Host "[1/3] Restoring solution packages $SolutionPath ..."
dotnet restore $SolutionPath

Write-Host "[2/3] Building solution $SolutionPath ..."
dotnet build $SolutionPath --no-restore --configuration $configuration

Write-Host "[3/3] Packing solution $SolutionPath to $AbsoluteOutputPath ..."
dotnet pack $SolutionPath --no-build --configuration $Configuration --output $OutputPath