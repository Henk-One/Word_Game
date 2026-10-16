# =========================================================================
# LEARNINGPATH: CENTRAL BUILD, AUTOMATED TEST RUNNER & LIVE CODE COVERAGE
# Coordinates frontend compile, backend test layers, and E2E automation.
# Ensures coverlet.collector presence and parses Cobertura metrics live.
# USAGE:
# C:\Repo\learningpath
# powershell -NoProfile -ExecutionPolicy Bypass -File scripts\build.ps1
# =========================================================================

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Path $PSScriptRoot -Parent
Set-Location $repoRoot

# Centralized Coverage Report Directory
$coverageReportDir = Join-Path $repoRoot "sandbox\reports\coverage"
if (!(Test-Path $coverageReportDir)) {
    New-Item -ItemType Directory -Path $coverageReportDir | Out-Null
}

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " STARTING SYSTEM COMPILATION & TEST PIPELINE " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# -------------------------------------------------------------------------
# STAGE 0: Self-Healing Testing Infrastructure Guard
# -------------------------------------------------------------------------
Write-Host "`n[STAGE 0] Verifying Coverlet telemetry collectors across endpoints..." -ForegroundColor Yellow
$testProjects = @(
    "services/Leaderboard.Api/tests/Unit/Leaderboard.Api.UnitTests.csproj",
    "services/Leaderboard.Api/tests/Integration/Leaderboard.Api.IntegrationTests.csproj",
    "services/Game.Api/tests/Unit/Game.Api.UnitTests.csproj",
    "services/Game.Api/tests/Contract/Game.Api.ContractTests.csproj"
)

foreach ($proj in $testProjects) {
    $content = Get-Content (Join-Path $repoRoot $proj) -Raw
    if ($content -notlike "*coverlet.collector*") {
        Write-Host "-> Injecting missing coverlet.collector into: $proj" -ForegroundColor Gray
        dotnet add $proj package coverlet.collector | Out-Null
    }
}

# -------------------------------------------------------------------------
# STAGE 1: Frontend Build & Unit Validation
# -------------------------------------------------------------------------
Write-Host "`n[STAGE 1] Validating and Building Frontend App..." -ForegroundColor Yellow
$frontendPath = Join-Path $repoRoot "frontend\game"

if (Test-Path $frontendPath) {
    Set-Location $frontendPath
    npm run build
    if ($LASTEXITCODE -ne 0) { throw "Frontend build or unit tests failed." }
} else {
    throw "Frontend path not found at $frontendPath"
}

# -------------------------------------------------------------------------
# STAGE 2: Backend Compilation & Isolated Test Layers (With Coverage)
# -------------------------------------------------------------------------
Set-Location $repoRoot
Write-Host "`n[STAGE 2] Running Backend Unit, Integration & Contract Tests..." -ForegroundColor Yellow

$coverageArgs = @(
    "--collect"
    "XPlat Code Coverage"
)

# Helper function to run test and return parsed line coverage percentage
function Run-TestWithCoverage {
    param([string]$ProjectName, [string]$OutputSubDir)

    $targetDir = Join-Path $coverageReportDir $OutputSubDir
    if (Test-Path $targetDir) { Remove-Item $targetDir -Recurse -Force | Out-Null }

    Write-Host "Executing tests for $OutputSubDir..." -ForegroundColor Cyan
    dotnet test $ProjectName --configuration Release @coverageArgs --results-directory $targetDir `
        -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura |
        Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "Tests failed for $ProjectName."
    }

    # Locate Cobertura report XML file
    $reportFile = Get-ChildItem -Path $targetDir -Filter "coverage.cobertura.xml" -Recurse | Select-Object -First 1
    if ($reportFile) {
        [xml]$xml = Get-Content $reportFile.FullName
        $lineRate = [double]$xml.coverage.'line-rate'
        $percentage = [Math]::Round($lineRate * 100, 2)
        return $percentage
    }
    return $null
}

$covLeaderboardUnit = Run-TestWithCoverage "services/Leaderboard.Api/tests/Unit/Leaderboard.Api.UnitTests.csproj" "leaderboard-unit"
$covLeaderboardInt  = Run-TestWithCoverage "services/Leaderboard.Api/tests/Integration/Leaderboard.Api.IntegrationTests.csproj" "leaderboard-integration"
$covGameUnit        = Run-TestWithCoverage "services/Game.Api/tests/Unit/Game.Api.UnitTests.csproj" "game-unit"
$covGameContract    = Run-TestWithCoverage "services/Game.Api/tests/Contract/Game.Api.ContractTests.csproj" "game-contract"

# Output Coverage Metrics Live to Console
Write-Host "`n--- SYSTEM CODE COVERAGE SUMMARY ---" -ForegroundColor DarkGray
function Print-Metric {
    param(
        [string]$Name,
        [double]$Value
    )

    $color = if ($Value -ge 80) { "Green" } elseif ($Value -ge 50) { "Yellow" } else { "Red" }
    Write-Host ("{0,-30} : " -f $Name) -NoNewline
    Write-Host "$Value%" -ForegroundColor $color
}
if ($null -ne $covLeaderboardUnit) { Print-Metric "Leaderboard.Api Unit" $covLeaderboardUnit }
if ($null -ne $covLeaderboardInt)  { Print-Metric "Leaderboard.Api Integration" $covLeaderboardInt }
if ($null -ne $covGameUnit)        { Print-Metric "Game.Api Unit" $covGameUnit }
if ($null -ne $covGameContract)    { Print-Metric "Game.Api Contract" $covGameContract }

# -------------------------------------------------------------------------
# STAGE 3: Spin up Live APIs for Workflow & End-to-End UI Testing
# -------------------------------------------------------------------------
Write-Host "`n[STAGE 3] Launching live background service replicas for E2E validation..." -ForegroundColor Yellow

$leaderboardProcess = $null
$gameProcess = $null
$frontendProcess = $null

try {
    Write-Host "-> Spawning Leaderboard.Api (http://localhost:5000)..." -ForegroundColor Cyan
    $leaderboardProcess = Start-Process dotnet -ArgumentList "run --project services/Leaderboard.Api/Leaderboard.Api.csproj --launch-profile Leaderboard.Api" -NoNewWindow -PassThru

    Write-Host "-> Spawning Game.Api (http://localhost:5151)..." -ForegroundColor Cyan
    $gameProcess = Start-Process dotnet -ArgumentList "run --project services/Game.Api/Game.Api.csproj --launch-profile Game.Api" -NoNewWindow -PassThru

    Write-Host "-> Spawning Frontend static server (http://localhost:8080)..." -ForegroundColor Cyan
    $frontendServerScript = Join-Path $frontendPath "node_modules\http-server\bin\http-server"
    if (-not (Test-Path -LiteralPath $frontendServerScript -PathType Leaf)) {
        throw "Frontend server executable not found at $frontendServerScript. Install frontend dependencies first."
    }
    $nodeExecutable = (Get-Command node.exe -ErrorAction Stop).Source
    $frontendProcess = Start-Process $nodeExecutable `
        -ArgumentList @("node_modules\http-server\bin\http-server", "dist", "-p", "8080", "-c-1") `
        -WorkingDirectory $frontendPath -NoNewWindow -PassThru

    Write-Host "Waiting for network ports to settle..." -ForegroundColor Gray
    Start-Sleep -Seconds 5

    # -------------------------------------------------------------------------
    # STAGE 4: Execute Real Network Integration & UI End-to-End Tests
    # -------------------------------------------------------------------------
    Set-Location $repoRoot
    Write-Host "`n[STAGE 4] Executing Live E2E Integration and Browser Scenarios..." -ForegroundColor Yellow

    Write-Host "-> Running Game.Api Live API Journey Tests..." -ForegroundColor Cyan
    $env:GAME_API_TEST_URL = "http://localhost:5151/api/game"
    dotnet test services/Game.Api/tests/E2E/Api/Game.Api.E2EApiTests.csproj --configuration Release
    if ($LASTEXITCODE -ne 0) { throw "Game.Api live API tests failed." }

    Write-Host "-> Running Playwright Web UI and Accessibility Automation..." -ForegroundColor Cyan
    Set-Location $frontendPath
    npx playwright test
    if ($LASTEXITCODE -ne 0) { throw "Playwright E2E UI testing layer failed." }

    Write-Host "-> Building and Testing WPF Desktop Client App via FlaUI..." -ForegroundColor Cyan
    Set-Location $repoRoot
    dotnet build desktop/game/src/Desktop.Game.csproj --configuration Release
    if ($LASTEXITCODE -ne 0) { throw "Desktop client build failed." }

    $env:WORD_GUESS_DESKTOP_EXE = Join-Path $repoRoot "desktop\game\src\bin\Release\net10.0-windows\Desktop.Game.exe"
    dotnet test desktop/game/tests/Desktop.Game.Tests.csproj --configuration Release
    if ($LASTEXITCODE -ne 0) { throw "Desktop UI and API workflow tests failed." }

    Write-Host "`n=========================================" -ForegroundColor Green
    Write-Host " SUCCESS: ALL COMPILATION & TEST LAYERS PASSED " -ForegroundColor Green
    Write-Host "=========================================" -ForegroundColor Green

}
finally {
    # -------------------------------------------------------------------------
    # STAGE 5: Strict Infrastructure Tear Down & Process Cleanup
    # -------------------------------------------------------------------------
    Write-Host "`n[STAGE 5] Tearing down background service instances..." -ForegroundColor Gray

    Remove-Item env:GAME_API_TEST_URL -ErrorAction SilentlyContinue
    Remove-Item env:WORD_GUESS_DESKTOP_EXE -ErrorAction SilentlyContinue

    if ($leaderboardProcess) { Stop-Process -Id $leaderboardProcess.Id -Force -ErrorAction SilentlyContinue }
    if ($gameProcess) { Stop-Process -Id $gameProcess.Id -Force -ErrorAction SilentlyContinue }
    if ($frontendProcess) { Stop-Process -Id $frontendProcess.Id -Force -ErrorAction SilentlyContinue }

    Get-Process -Name "node" -ErrorAction SilentlyContinue |
        Where-Object { $_.Path -like "*frontend*" } |
        Stop-Process -Force -ErrorAction SilentlyContinue

    Set-Location $repoRoot
}
