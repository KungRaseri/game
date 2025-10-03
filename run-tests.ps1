# PowerShell script to run tests with organized results
param(
    [string]$Project = "",
    [switch]$Coverage = $false,
    [switch]$Clean = $false
)

# Clean previous test results if requested
if ($Clean) {
    Write-Host "Cleaning previous test results..." -ForegroundColor Yellow
    Remove-Item -Path ".\TestResults" -Recurse -Force -ErrorAction SilentlyContinue
}

# Build test command arguments
$testArgs = @()

# Add project if specified
if ($Project) {
    $testArgs += $Project
}

# Always use our settings file
$testArgs += "--settings"
$testArgs += "coverlet.runsettings"

# Add logger for TRX output
$testArgs += "--logger"
$testArgs += "trx"

# Add coverage collection if requested
if ($Coverage) {
    $testArgs += "--collect:XPlat Code Coverage"
}

# Run the tests
Write-Host "Running tests with the following arguments: $($testArgs -join ' ')" -ForegroundColor Green
& dotnet test $testArgs

# Display results summary
Write-Host "`nTest Results Directory Structure:" -ForegroundColor Cyan
if (Test-Path ".\TestResults") {
    Get-ChildItem -Path ".\TestResults" -Directory | ForEach-Object {
        Write-Host "  $($_.Name)/" -ForegroundColor White
        Get-ChildItem -Path $_.FullName -File "*.trx" | ForEach-Object {
            Write-Host "    $($_.Name)" -ForegroundColor Gray
        }
    }
} else {
    Write-Host "  No test results found." -ForegroundColor Red
}
