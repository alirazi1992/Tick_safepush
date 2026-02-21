#Requires -Version 5.1
# TikQ Backend - IIS deployment script (production-safe, idempotent)
# Run from: same folder as Ticketing.Backend.csproj (or any folder; uses $PSScriptRoot for project path)
$ErrorActionPreference = "Stop"

# --- Config (safe variable names; no secrets) ---
$AppPoolName     = "TikQ"
$SiteName        = "TikQ"
$PublishRoot     = "C:\publish"
$SitePort        = 8080
$HealthUrl       = "http://localhost:8080/api/health"
$JwtSecretEnvVar = "TikQ_JWT_SECRET"
$MinSecretLength = 32

$ProjectPath     = Join-Path $PSScriptRoot "Ticketing.Backend.csproj"
$Timestamp       = Get-Date -Format "yyyyMMdd-HHmmss"
$PublishDir      = Join-Path $PublishRoot "tikq-backend-$Timestamp"

# --- Helpers ---
function Write-Step { param([string]$Message) Write-Host "[$(Get-Date -Format 'HH:mm:ss')] $Message" }
function Ensure-Dir  { param([string]$Path) if (-not (Test-Path $Path)) { New-Item -ItemType Directory -Path $Path -Force | Out-Null; Write-Step "Created: $Path" } }

function Grant-AppPoolFullControl {
    param([string]$TargetPath, [string]$AppPoolName)
    $principal = "IIS AppPool\${AppPoolName}"
    # Use -f to avoid PowerShell parsing "$principal:(OI)(CI)F" as invalid variable scope
    $grantArg = "{0}:(OI)(CI)F" -f $principal
    icacls $TargetPath /grant $grantArg
}

# --- 1. Validate JWT secret (must exist, >= 32 chars; never log value) ---
Write-Step "Validating $JwtSecretEnvVar ..."
$secret = [Environment]::GetEnvironmentVariable($JwtSecretEnvVar, "Process")
if (-not $secret) { $secret = [Environment]::GetEnvironmentVariable($JwtSecretEnvVar, "Machine") }
if (-not $secret) { $secret = [Environment]::GetEnvironmentVariable($JwtSecretEnvVar, "User") }
if (-not $secret) {
    Write-Host "ERROR: Environment variable '$JwtSecretEnvVar' is not set. Set it before deploying (e.g. [Environment]::SetEnvironmentVariable('$JwtSecretEnvVar','your-secret','Machine'))." -ForegroundColor Red
    throw "Validation failed: $JwtSecretEnvVar is missing."
}
if ($secret.Length -lt $MinSecretLength) {
    Write-Host "ERROR: $JwtSecretEnvVar must be at least $MinSecretLength characters (current length: $($secret.Length))." -ForegroundColor Red
    throw "Validation failed: $JwtSecretEnvVar too short."
}
Write-Step "JWT secret validated (length >= $MinSecretLength)."

# --- 1b. Bootstrap env vars (optional; if password set, validate and collect for injection) ---
$MinBootstrapPasswordLength = 8

# Admin
$BootstrapAdminPasswordEnvVar = "TikQ_BOOTSTRAP_ADMIN_PASSWORD"
$BootstrapAdminEmailEnvVar    = "TikQ_BOOTSTRAP_ADMIN_EMAIL"
$bootstrapAdminPassword = [Environment]::GetEnvironmentVariable($BootstrapAdminPasswordEnvVar, "Process")
if (-not $bootstrapAdminPassword) { $bootstrapAdminPassword = [Environment]::GetEnvironmentVariable($BootstrapAdminPasswordEnvVar, "Machine") }
if (-not $bootstrapAdminPassword) { $bootstrapAdminPassword = [Environment]::GetEnvironmentVariable($BootstrapAdminPasswordEnvVar, "User") }
$bootstrapAdminEmail = [Environment]::GetEnvironmentVariable($BootstrapAdminEmailEnvVar, "Process")
if (-not $bootstrapAdminEmail) { $bootstrapAdminEmail = [Environment]::GetEnvironmentVariable($BootstrapAdminEmailEnvVar, "Machine") }
if (-not $bootstrapAdminEmail) { $bootstrapAdminEmail = [Environment]::GetEnvironmentVariable($BootstrapAdminEmailEnvVar, "User") }
if ($bootstrapAdminPassword) {
    if ($bootstrapAdminPassword.Length -lt $MinBootstrapPasswordLength) {
        Write-Host "ERROR: $BootstrapAdminPasswordEnvVar is set but must be at least $MinBootstrapPasswordLength characters (current length: $($bootstrapAdminPassword.Length))." -ForegroundColor Red
        throw "Validation failed: $BootstrapAdminPasswordEnvVar too short."
    }
    if ([string]::IsNullOrWhiteSpace($bootstrapAdminEmail)) { $bootstrapAdminEmail = "admin@local" }
    else { $bootstrapAdminEmail = $bootstrapAdminEmail.Trim() }
    Write-Step "Bootstrap admin env vars present; will inject (password length >= $MinBootstrapPasswordLength, email set)."
}

# Client
$BootstrapClientPasswordEnvVar = "TikQ_BOOTSTRAP_CLIENT_PASSWORD"
$BootstrapClientEmailEnvVar    = "TikQ_BOOTSTRAP_CLIENT_EMAIL"
$bootstrapClientPassword = [Environment]::GetEnvironmentVariable($BootstrapClientPasswordEnvVar, "Process")
if (-not $bootstrapClientPassword) { $bootstrapClientPassword = [Environment]::GetEnvironmentVariable($BootstrapClientPasswordEnvVar, "Machine") }
if (-not $bootstrapClientPassword) { $bootstrapClientPassword = [Environment]::GetEnvironmentVariable($BootstrapClientPasswordEnvVar, "User") }
$bootstrapClientEmail = [Environment]::GetEnvironmentVariable($BootstrapClientEmailEnvVar, "Process")
if (-not $bootstrapClientEmail) { $bootstrapClientEmail = [Environment]::GetEnvironmentVariable($BootstrapClientEmailEnvVar, "Machine") }
if (-not $bootstrapClientEmail) { $bootstrapClientEmail = [Environment]::GetEnvironmentVariable($BootstrapClientEmailEnvVar, "User") }
if ($bootstrapClientPassword) {
    if ($bootstrapClientPassword.Length -lt $MinBootstrapPasswordLength) {
        Write-Host "ERROR: $BootstrapClientPasswordEnvVar is set but must be at least $MinBootstrapPasswordLength characters (current length: $($bootstrapClientPassword.Length))." -ForegroundColor Red
        throw "Validation failed: $BootstrapClientPasswordEnvVar too short."
    }
    if ([string]::IsNullOrWhiteSpace($bootstrapClientEmail)) { $bootstrapClientEmail = "client@local" }
    else { $bootstrapClientEmail = $bootstrapClientEmail.Trim() }
    Write-Step "Bootstrap client env vars present; will inject (password length >= $MinBootstrapPasswordLength, email set)."
}

# Technician
$BootstrapTechPasswordEnvVar = "TikQ_BOOTSTRAP_TECH_PASSWORD"
$BootstrapTechEmailEnvVar    = "TikQ_BOOTSTRAP_TECH_EMAIL"
$bootstrapTechPassword = [Environment]::GetEnvironmentVariable($BootstrapTechPasswordEnvVar, "Process")
if (-not $bootstrapTechPassword) { $bootstrapTechPassword = [Environment]::GetEnvironmentVariable($BootstrapTechPasswordEnvVar, "Machine") }
if (-not $bootstrapTechPassword) { $bootstrapTechPassword = [Environment]::GetEnvironmentVariable($BootstrapTechPasswordEnvVar, "User") }
$bootstrapTechEmail = [Environment]::GetEnvironmentVariable($BootstrapTechEmailEnvVar, "Process")
if (-not $bootstrapTechEmail) { $bootstrapTechEmail = [Environment]::GetEnvironmentVariable($BootstrapTechEmailEnvVar, "Machine") }
if (-not $bootstrapTechEmail) { $bootstrapTechEmail = [Environment]::GetEnvironmentVariable($BootstrapTechEmailEnvVar, "User") }
if ($bootstrapTechPassword) {
    if ($bootstrapTechPassword.Length -lt $MinBootstrapPasswordLength) {
        Write-Host "ERROR: $BootstrapTechPasswordEnvVar is set but must be at least $MinBootstrapPasswordLength characters (current length: $($bootstrapTechPassword.Length))." -ForegroundColor Red
        throw "Validation failed: $BootstrapTechPasswordEnvVar too short."
    }
    if ([string]::IsNullOrWhiteSpace($bootstrapTechEmail)) { $bootstrapTechEmail = "tech@local" }
    else { $bootstrapTechEmail = $bootstrapTechEmail.Trim() }
    Write-Step "Bootstrap technician env vars present; will inject (password length >= $MinBootstrapPasswordLength, email set)."
}

# Supervisor
$BootstrapSupervisorPasswordEnvVar = "TikQ_BOOTSTRAP_SUPERVISOR_PASSWORD"
$BootstrapSupervisorEmailEnvVar    = "TikQ_BOOTSTRAP_SUPERVISOR_EMAIL"
$bootstrapSupervisorPassword = [Environment]::GetEnvironmentVariable($BootstrapSupervisorPasswordEnvVar, "Process")
if (-not $bootstrapSupervisorPassword) { $bootstrapSupervisorPassword = [Environment]::GetEnvironmentVariable($BootstrapSupervisorPasswordEnvVar, "Machine") }
if (-not $bootstrapSupervisorPassword) { $bootstrapSupervisorPassword = [Environment]::GetEnvironmentVariable($BootstrapSupervisorPasswordEnvVar, "User") }
$bootstrapSupervisorEmail = [Environment]::GetEnvironmentVariable($BootstrapSupervisorEmailEnvVar, "Process")
if (-not $bootstrapSupervisorEmail) { $bootstrapSupervisorEmail = [Environment]::GetEnvironmentVariable($BootstrapSupervisorEmailEnvVar, "Machine") }
if (-not $bootstrapSupervisorEmail) { $bootstrapSupervisorEmail = [Environment]::GetEnvironmentVariable($BootstrapSupervisorEmailEnvVar, "User") }
if ($bootstrapSupervisorPassword) {
    if ($bootstrapSupervisorPassword.Length -lt $MinBootstrapPasswordLength) {
        Write-Host "ERROR: $BootstrapSupervisorPasswordEnvVar is set but must be at least $MinBootstrapPasswordLength characters (current length: $($bootstrapSupervisorPassword.Length))." -ForegroundColor Red
        throw "Validation failed: $BootstrapSupervisorPasswordEnvVar too short."
    }
    if ([string]::IsNullOrWhiteSpace($bootstrapSupervisorEmail)) { $bootstrapSupervisorEmail = "supervisor@local" }
    else { $bootstrapSupervisorEmail = $bootstrapSupervisorEmail.Trim() }
    Write-Step "Bootstrap supervisor env vars present; will inject (password length >= $MinBootstrapPasswordLength, email set)."
}

# --- 2. Ensure publish root ---
Ensure-Dir $PublishRoot

# --- 3. Publish ASP.NET Core 8 (Release) ---
Write-Step "Publishing to $PublishDir ..."
if (-not (Test-Path $ProjectPath)) { throw "Project not found: $ProjectPath" }
dotnet publish $ProjectPath -c Release -o $PublishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }
Write-Step "Publish completed."

# --- 4. Ensure required folders in publish dir ---
Ensure-Dir (Join-Path $PublishDir "logs")
Ensure-Dir (Join-Path $PublishDir "App_Data")
Ensure-Dir (Join-Path $PublishDir "App_Data\keys")

# --- 5. Inject environment variables into web.config (idempotent; no secrets in logs) ---
$WebConfigPath = Join-Path $PublishDir "web.config"
if (-not (Test-Path $WebConfigPath)) { throw "web.config not found after publish: $WebConfigPath" }
[xml]$webConfig = Get-Content $WebConfigPath -Encoding UTF8
$aspNetCore = $webConfig.configuration.location.'system.webServer'.aspNetCore
if (-not $aspNetCore) { $aspNetCore = $webConfig.configuration.'system.webServer'.aspNetCore }
if (-not $aspNetCore) { throw "Could not find aspNetCore node in web.config" }

# Build hashtable of name -> value to inject (never log values)
$varsToInject = @{ "Jwt__Secret" = $secret }
if ($bootstrapAdminPassword) {
    $varsToInject["TikQ_BOOTSTRAP_ADMIN_PASSWORD"] = $bootstrapAdminPassword
    $varsToInject["TikQ_BOOTSTRAP_ADMIN_EMAIL"]    = $bootstrapAdminEmail
}
if ($bootstrapClientPassword) {
    $varsToInject["TikQ_BOOTSTRAP_CLIENT_PASSWORD"] = $bootstrapClientPassword
    $varsToInject["TikQ_BOOTSTRAP_CLIENT_EMAIL"]    = $bootstrapClientEmail
}
if ($bootstrapTechPassword) {
    $varsToInject["TikQ_BOOTSTRAP_TECH_PASSWORD"] = $bootstrapTechPassword
    $varsToInject["TikQ_BOOTSTRAP_TECH_EMAIL"]    = $bootstrapTechEmail
}
if ($bootstrapSupervisorPassword) {
    $varsToInject["TikQ_BOOTSTRAP_SUPERVISOR_PASSWORD"] = $bootstrapSupervisorPassword
    $varsToInject["TikQ_BOOTSTRAP_SUPERVISOR_EMAIL"]    = $bootstrapSupervisorEmail
}

# Ensure <environmentVariables> exists under <aspNetCore>
$envVars = $aspNetCore.environmentVariables
if (-not $envVars) {
    $envVars = $webConfig.CreateElement("environmentVariables")
    [void]$aspNetCore.AppendChild($envVars)
}

# Update or add each variable (idempotent: replace existing entries by name)
foreach ($varName in $varsToInject.Keys) {
    $value = $varsToInject[$varName]
    $existing = $envVars.environmentVariable | Where-Object { $_.name -eq $varName }
    if ($existing) {
        $existing.value = $value
    } else {
        $el = $webConfig.CreateElement("environmentVariable")
        $el.SetAttribute("name", $varName)
        $el.SetAttribute("value", $value)
        [void]$envVars.AppendChild($el)
    }
}

# Ensure ConnectionStrings__DefaultConnection in <environmentVariables> (add only if missing)
$connStrName = "ConnectionStrings__DefaultConnection"
$connStrValue = "Data Source=C:\TikQData\ticketing.db"
$existingConnStr = $envVars.environmentVariable | Where-Object { $_.name -eq $connStrName }
if (-not $existingConnStr) {
    $elConn = $webConfig.CreateElement("environmentVariable")
    $elConn.SetAttribute("name", $connStrName)
    $elConn.SetAttribute("value", $connStrValue)
    [void]$envVars.AppendChild($elConn)
}

$webConfig.Save($WebConfigPath)
$injectedNames = $varsToInject.Keys -join ", "
$lengths = ($varsToInject.GetEnumerator() | ForEach-Object { "$($_.Key).Length=$($_.Value.Length)" }) -join "; "
Write-Step "Injected into web.config: $injectedNames ($lengths)."

# --- 6. Grant full control to IIS AppPool identity ---
Write-Step "Granting permissions to IIS AppPool\${AppPoolName} on $PublishDir ..."
Grant-AppPoolFullControl -TargetPath $PublishDir -AppPoolName $AppPoolName
foreach ($sub in @("logs", "App_Data", "App_Data\keys")) {
    $subPath = Join-Path $PublishDir $sub
    if (Test-Path $subPath) { Grant-AppPoolFullControl -TargetPath $subPath -AppPoolName $AppPoolName }
}
Write-Step "Permissions set."

# --- 7. Ensure IIS module loaded ---
if (-not (Get-Module -ListAvailable -Name WebAdministration)) {
    Write-Host "WARNING: WebAdministration module not found. Install IIS management tools or run from a machine with IIS." -ForegroundColor Yellow
}
Import-Module WebAdministration -ErrorAction Stop

# --- 8. Create or update site/app and point to new folder ---
$sitePath = "IIS:\Sites\$SiteName"
if (-not (Test-Path $sitePath)) {
    Write-Step "Creating site ${SiteName} on port $SitePort ..."
    New-Website -Name $SiteName -PhysicalPath $PublishDir -Port $SitePort -ApplicationPool $AppPoolName
} else {
    Write-Step "Updating site ${SiteName} physical path to $PublishDir ..."
    Set-ItemProperty -Path $sitePath -Name physicalPath -Value $PublishDir
}
# Ensure app pool exists and is assigned
$poolPath = "IIS:\AppPools\$AppPoolName"
if (-not (Test-Path $poolPath)) {
    Write-Step "Creating application pool ${AppPoolName} ..."
    New-WebAppPool -Name $AppPoolName
}
Set-ItemProperty -Path $sitePath -Name applicationPool -Value $AppPoolName -ErrorAction SilentlyContinue
Write-Step "IIS site/app pointed to $PublishDir."

# --- 9. Restart AppPool ---
Write-Step "Recycling application pool ${AppPoolName} ..."
Restart-WebAppPool -Name $AppPoolName

# --- 10. Restart IIS (optional; recycle often enough; uncomment if required) ---
Write-Step "Restarting IIS ..."
iisreset

# --- 11. Smoke test ---
Write-Step "Waiting 5s for app to start..."
Start-Sleep -Seconds 5
Write-Step "Smoke test: GET $HealthUrl ..."
try {
    $response = Invoke-WebRequest -Uri $HealthUrl -UseBasicParsing -TimeoutSec 15
    if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 300) {
        Write-Host "Smoke test PASSED (HTTP $($response.StatusCode))." -ForegroundColor Green
    } else {
        Write-Host "Smoke test WARNING: HTTP $($response.StatusCode)." -ForegroundColor Yellow
    }
} catch {
    Write-Host "Smoke test FAILED: $_" -ForegroundColor Red
    throw "Smoke test failed: $_"
}

# --- 12. Show latest stdout log (first 200 lines) ---
$logsDir = Join-Path $PublishDir "logs"
$latestLog = Get-ChildItem -Path $logsDir -Filter "stdout*.log" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $latestLog) { $latestLog = Get-ChildItem -Path $logsDir -Filter "*.log" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1 }
if ($latestLog) {
    Write-Step "Latest log (first 200 lines): $($latestLog.FullName)"
    Get-Content $latestLog.FullName -TotalCount 200 -ErrorAction SilentlyContinue
} else {
    Write-Step "No stdout log found in $logsDir yet (app may still be starting)."
}

Write-Host ""
Write-Step "Deploy completed. Publish folder: $PublishDir"
