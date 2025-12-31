# run-backend.ps1
# Safely stops any running backend and starts a fresh instance
# Prevents MSB3027/MSB3021 file-lock errors on Windows

param(
    [int]$Port = 5000
)

$ErrorActionPreference = "Stop"

# Get script directory and project root
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path $scriptDir
$backendPath = Join-Path $repoRoot "backend\Ticketing.Backend"

Write-Host "=== TikQ Backend Runner ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Stop any running backend processes
Write-Host "Step 1: Stopping any running backend processes..." -ForegroundColor Yellow
Write-Host ""
& "$scriptDir\stop-backend.ps1"
Write-Host ""

# Step 2: Wait a moment for processes to fully stop
Start-Sleep -Milliseconds 500

# Step 3: Check if port is available, fallback to 5001 if needed
$selectedPort = $Port
$portAvailable = $false

Write-Host "Step 2: Checking port availability..." -ForegroundColor Yellow

try {
    $connections = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
    if (-not $connections) {
        $portAvailable = $true
        Write-Host "  Port $Port is available" -ForegroundColor Green
    } else {
        Write-Host "  Port $Port is still in use" -ForegroundColor Yellow
        if ($Port -eq 5000) {
            Write-Host "  Trying fallback port 5001..." -ForegroundColor Yellow
            $connections5001 = Get-NetTCPConnection -LocalPort 5001 -State Listen -ErrorAction SilentlyContinue
            if (-not $connections5001) {
                $selectedPort = 5001
                $portAvailable = $true
                Write-Host "  Port 5001 is available (fallback)" -ForegroundColor Green
            } else {
                Write-Host "  ERROR: Both ports 5000 and 5001 are in use" -ForegroundColor Red
                Write-Host "  Please stop the processes using these ports manually" -ForegroundColor Red
                exit 1
            }
        } else {
            Write-Host "  ERROR: Port $Port is in use" -ForegroundColor Red
            exit 1
        }
    }
} catch {
    Write-Host "  Could not check port availability, proceeding anyway..." -ForegroundColor Yellow
    $portAvailable = $true
}

if (-not $portAvailable) {
    Write-Host ""
    Write-Host "ERROR: Cannot start backend - port conflict" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 4: Verify backend directory exists
if (-not (Test-Path $backendPath)) {
    Write-Host "ERROR: Backend path not found: $backendPath" -ForegroundColor Red
    exit 1
}

# Step 5: Check if .NET SDK is available
try {
    $dotnetVersion = dotnet --version 2>&1
    Write-Host "Using .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "ERROR: .NET SDK not found. Please install .NET 8 SDK." -ForegroundColor Red
    exit 1
}

# Step 6: Determine which project to run
# Note: Ticketing.Api.csproj is a library (controllers only), not runnable
# The actual runnable project is Ticketing.Backend.csproj in the root
$projectFile = Join-Path $backendPath "Ticketing.Backend.csproj"
if (-not (Test-Path $projectFile)) {
    Write-Host "ERROR: Project file not found: $projectFile" -ForegroundColor Red
    exit 1
}

# Navigate to backend directory
Push-Location $backendPath

try {
    Write-Host "Step 3: Starting backend server..." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Backend directory: $backendPath" -ForegroundColor Gray
    Write-Host "Project: Ticketing.Backend.csproj" -ForegroundColor Gray
    Write-Host "URL: http://127.0.0.1:$selectedPort" -ForegroundColor White
    if ($selectedPort -ne $Port) {
        Write-Host "  (Note: Port $Port was in use, using $selectedPort instead)" -ForegroundColor Yellow
    }
    Write-Host "Swagger: http://127.0.0.1:$selectedPort/swagger" -ForegroundColor White
    Write-Host ""
    Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Gray
    Write-Host ""
    
    # Set environment variable for URL
    $env:ASPNETCORE_URLS = "http://127.0.0.1:$selectedPort"
    
    # Run the backend
    dotnet run --project $projectFile
    
} catch {
    Write-Host ""
    Write-Host "ERROR: Failed to start backend: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Ensure .NET 8 SDK is installed" -ForegroundColor White
    Write-Host "  2. Run 'dotnet restore' in the backend directory" -ForegroundColor White
    Write-Host "  3. Run '.\tools\stop-backend.ps1' to stop any stale processes" -ForegroundColor White
    Write-Host "  4. Check backend logs above for specific errors" -ForegroundColor White
    exit 1
} finally {
    Pop-Location
}
