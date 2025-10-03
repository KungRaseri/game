# PowerShell script to clean test results
Write-Host "Cleaning test results..." -ForegroundColor Yellow

$testResultsPath = ".\TestResults"

if (Test-Path $testResultsPath) {
    try {
        Remove-Item -Path $testResultsPath -Recurse -Force
        Write-Host "✅ Test results cleaned successfully!" -ForegroundColor Green
    } catch {
        Write-Host "❌ Error cleaning test results: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "ℹ️ No test results to clean." -ForegroundColor Blue
}

Write-Host "Test results directory is ready for fresh test runs." -ForegroundColor Cyan
