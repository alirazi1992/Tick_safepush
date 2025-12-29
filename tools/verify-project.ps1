# verify-project.ps1
# Verifies project integrity after cleanup

$ErrorActionPreference = "Continue"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptRoot
$frontendPath = Join-Path $projectRoot "frontend"
$backendPath = Join-Path $projectRoot "backend\Ticketing.Backend"

Write-Host "🔍 Verifying project integrity..." -ForegroundColor Cyan
$separator = "=" * 60
Write-Host $separator -ForegroundColor Cyan

$errors = @()
$warnings = @()
$success = @()

# Check git status
Write-Host "`n📋 Checking git status..." -ForegroundColor Yellow
Push-Location $projectRoot
try {
    $gitStatus = git status --porcelain 2>&1
    if ($LASTEXITCODE -eq 0) {
        if ($gitStatus) {
            $modifiedCount = ($gitStatus -split "`n" | Where-Object { $_ -match "^ M" }).Count
            $untrackedCount = ($gitStatus -split "`n" | Where-Object { $_ -match "^\?\?" }).Count
            Write-Host "⚠️  Uncommitted changes detected:" -ForegroundColor Yellow
            Write-Host "  Modified: $modifiedCount files" -ForegroundColor Gray
            Write-Host "  Untracked: $untrackedCount files" -ForegroundColor Gray
            $warnings += "Git working directory has uncommitted changes"
        } else {
            Write-Host "✅ Git working directory is clean" -ForegroundColor Green
            $success += "Git status clean"
        }
    } else {
        Write-Host "⚠️  Git check failed (may not be a git repo)" -ForegroundColor Yellow
        $warnings += "Git check failed"
    }
} catch {
    Write-Host "⚠️  Git check error: $_" -ForegroundColor Yellow
    $warnings += "Git check error"
}
Pop-Location

# Verify frontend dependencies
Write-Host "`n📦 Verifying frontend dependencies..." -ForegroundColor Yellow
if (Test-Path $frontendPath) {
    Push-Location $frontendPath
    try {
        if (Test-Path "node_modules") {
            $pkgCount = (Get-ChildItem node_modules -Directory -ErrorAction SilentlyContinue).Count
            if ($pkgCount -gt 0) {
                Write-Host "✅ node_modules exists with $pkgCount package directories" -ForegroundColor Green
                $success += "Frontend: node_modules present"
            } else {
                Write-Host "⚠️  node_modules exists but appears empty" -ForegroundColor Yellow
                $warnings += "Frontend: node_modules empty"
            }
        } else {
            Write-Host "❌ node_modules missing!" -ForegroundColor Red
            $errors += "Frontend: node_modules missing"
        }
        
        # Check package.json
        if (Test-Path "package.json") {
            Write-Host "✅ package.json exists" -ForegroundColor Green
        } else {
            Write-Host "❌ package.json missing!" -ForegroundColor Red
            $errors += "Frontend: package.json missing"
        }
    } finally {
        Pop-Location
    }
} else {
    Write-Host "❌ Frontend path does not exist!" -ForegroundColor Red
    $errors += "Frontend: path missing"
}

# Verify backend can restore
Write-Host "`n📦 Verifying backend project..." -ForegroundColor Yellow
if (Test-Path $backendPath) {
    Push-Location $backendPath
    try {
        if (Test-Path "Ticketing.Backend.csproj") {
            Write-Host "✅ Project file exists" -ForegroundColor Green
            
            # Check if dotnet is available
            $dotnetCheck = Get-Command dotnet -ErrorAction SilentlyContinue
            if ($null -ne $dotnetCheck) {
                Write-Host "  Running dotnet restore (dry-run)..." -ForegroundColor Gray
                dotnet restore --no-build 2>&1 | Out-Null
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✅ Backend project valid" -ForegroundColor Green
                    $success += "Backend: project valid"
                } else {
                    Write-Host "⚠️  Backend restore had issues (exit code: $LASTEXITCODE)" -ForegroundColor Yellow
                    $warnings += "Backend: restore issues"
                }
            } else {
                Write-Host "⚠️  dotnet CLI not found (skipping restore check)" -ForegroundColor Yellow
                $warnings += "Backend: dotnet CLI not found"
            }
        } else {
            Write-Host "❌ Project file missing!" -ForegroundColor Red
            $errors += "Backend: csproj missing"
        }
    } finally {
        Pop-Location
    }
} else {
    Write-Host "❌ Backend path does not exist!" -ForegroundColor Red
    $errors += "Backend: path missing"
}

# Check VS Code settings
Write-Host "`n⚙️  Checking VS Code/Cursor settings..." -ForegroundColor Yellow
$vscodeSettings = Join-Path $projectRoot ".vscode\settings.json"
if (Test-Path $vscodeSettings) {
    Write-Host "✅ .vscode/settings.json exists" -ForegroundColor Green
    $success += "VS Code settings configured"
} else {
    Write-Host "⚠️  .vscode/settings.json not found" -ForegroundColor Yellow
    $warnings += "VS Code settings missing"
}

# Summary
$separator = "=" * 60
Write-Host "`n$separator" -ForegroundColor Cyan
Write-Host "VERIFICATION SUMMARY" -ForegroundColor Cyan
Write-Host $separator -ForegroundColor Cyan

if ($success.Count -gt 0) {
    Write-Host "`n✅ Success ($($success.Count)):" -ForegroundColor Green
    foreach ($item in $success) {
        Write-Host "  ✓ $item" -ForegroundColor Gray
    }
}

if ($warnings.Count -gt 0) {
    Write-Host "`n⚠️  Warnings ($($warnings.Count)):" -ForegroundColor Yellow
    foreach ($item in $warnings) {
        Write-Host "  ⚠ $item" -ForegroundColor Gray
    }
}

if ($errors.Count -gt 0) {
    Write-Host "`n❌ Errors ($($errors.Count)):" -ForegroundColor Red
    foreach ($item in $errors) {
        Write-Host "  ✗ $item" -ForegroundColor Gray
    }
    Write-Host "`n❌ Verification failed!" -ForegroundColor Red
    exit 1
} else {
    Write-Host "`n✅ All critical checks passed!" -ForegroundColor Green
    if ($warnings.Count -gt 0) {
        Write-Host "WARNING: Some warnings present - review above" -ForegroundColor Yellow
    }
}
Write-Host $separator -ForegroundColor Cyan

return @{
    Success = $success
    Warnings = $warnings
    Errors = $errors
}

