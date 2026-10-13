$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$testProjects = @(
    "services\Game.Api\tests\Unit\Game.Api.UnitTests.csproj",
    "services\Game.Api\tests\Contract\Game.Api.ContractTests.csproj",
    "services\Leaderboard.Api\tests\Unit\Leaderboard.Api.UnitTests.csproj",
    "services\Leaderboard.Api\tests\Integration\Leaderboard.Api.IntegrationTests.csproj"
)
$apiProjects = @(
    "services\Game.Api\Game.Api.csproj",
    "services\Leaderboard.Api\Leaderboard.Api.csproj"
)
$failedTests = [System.Collections.Generic.List[string]]::new()

foreach ($project in $testProjects) {
    $projectPath = Join-Path $repositoryRoot $project
    Write-Host "`n== Testing $project =="
    & dotnet test $projectPath
    if ($LASTEXITCODE -ne 0) {
        $failedTests.Add($project)
    }
}

$frontendDirectory = Join-Path $repositoryRoot "frontend\game"
Push-Location $frontendDirectory
try {
    Write-Host "`n== Testing frontend =="
    & npm test
    if ($LASTEXITCODE -ne 0) {
        $failedTests.Add("frontend\game")
    }
}
finally {
    Pop-Location
}

if ($failedTests.Count -gt 0) {
    [Console]::Error.WriteLine("Build stopped because tests failed: $($failedTests -join ', ')")
    exit 1
}

foreach ($project in $apiProjects) {
    $projectPath = Join-Path $repositoryRoot $project
    Write-Host "`n== Building $project =="
    & dotnet build $projectPath --no-restore
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

Push-Location $frontendDirectory
try {
    Write-Host "`n== Building frontend =="
    & npm run build:app
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}
finally {
    Pop-Location
}
