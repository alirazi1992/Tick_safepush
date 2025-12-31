# Run Backend Script
# Starts the TikQ backend API server
# Usage: .\tools\run-backend.ps1

$ErrorActionPreference = "Stop"

# Get script directory and project root
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
$backendPath = Join-Path $repoRoot "backend\Ticketing.Backend"

Write-Host "=== TikQ Backend Runner ===" -ForegroundColor Cyan
Write-Host ""

# Check if backend directory exists
if (-not (Test-Path $backendPath)) {
    Write-Host "ERROR: Backend path not found: $backendPath" -ForegroundColor Red
    exit 1
}

# Check if .NET SDK is available
try {
    $dotnetVersion = dotnet --version 2>&1
    Write-Host "Using .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "ERROR: .NET SDK not found. Please install .NET 8 SDK." -ForegroundColor Red
    exit 1
}

# Navigate to backend directory
Push-Location $backendPath

try {
    Write-Host "Backend directory: $backendPath" -ForegroundColor Gray
    Write-Host ""
    
    # Check if project file exists
    $projectFile = Join-Path $backendPath "Ticketing.Backend.csproj"
    if (-not (Test-Path $projectFile)) {
        Write-Host "ERROR: Project file not found: $projectFile" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Starting backend server..." -ForegroundColor Yellow
    Write-Host "Backend will be available at:" -ForegroundColor Gray
    Write-Host "  - HTTP:  http://localhost:5000" -ForegroundColor White
    Write-Host "  - HTTPS: https://localhost:7000" -ForegroundColor White
    Write-Host "  - Swagger: http://localhost:5000/swagger" -ForegroundColor White
    Write-Host ""
    Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Gray
    Write-Host ""
    
    # Run the backend
    dotnet run
    
} catch {
    Write-Host ""
    Write-Host "ERROR: Failed to start backend: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Ensure .NET 8 SDK is installed" -ForegroundColor White
    Write-Host "  2. Run 'dotnet restore' in the backend directory" -ForegroundColor White
    Write-Host "  3. Check that port 5000/7000 is not in use" -ForegroundColor White
    Write-Host "  4. Check backend logs above for specific errors" -ForegroundColor White
    exit 1
} finally {
    Pop-Location
}





