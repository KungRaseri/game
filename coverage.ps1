# PowerShell script to generate code coverage reports from centralized test results
param(
    [string]$Project = "",
    [switch]$Open = $false
)

Write-Host "Generating code coverage report..." -ForegroundColor Yellow

$reportPattern = if ($Project) {
    "TestResults/$Project/*/coverage.cobertura.xml"
} else {
    "TestResults/*/*/coverage.cobertura.xml"
}

$targetDir = "TestResults/CoverageReport"

# Check if any coverage files exist
$coverageFiles = Get-ChildItem -Path $reportPattern -ErrorAction SilentlyContinue
if ($coverageFiles.Count -eq 0) {
    Write-Host "❌ No coverage files found matching pattern: $reportPattern" -ForegroundColor Red
    Write-Host "Run tests with coverage first: .\run-tests.ps1 -Coverage" -ForegroundColor Yellow
    exit 1
}

Write-Host "Found $($coverageFiles.Count) coverage file(s)" -ForegroundColor Green

# Generate the report
try {
    & reportgenerator -reports:$reportPattern -targetdir:$targetDir -reporttypes:Html
    Write-Host "✅ Coverage report generated at: $targetDir\index.html" -ForegroundColor Green
    
    if ($Open) {
        Write-Host "Opening coverage report..." -ForegroundColor Cyan
        Start-Process "$targetDir\index.html"
    }
} catch {
    Write-Host "❌ Error generating coverage report: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}