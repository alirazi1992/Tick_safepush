# Verify Field Definitions API Endpoints
# This script tests the field definitions endpoints with proper authentication

param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$AdminEmail = "admin@example.com",
    [string]$AdminPassword = "Admin@123"
)

Write-Host "=== Field Definitions API Verification ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Login and get token
Write-Host "[1/5] Logging in as admin..." -ForegroundColor Yellow
$loginBody = @{
    email = $AdminEmail
    password = $AdminPassword
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" `
        -Method POST `
        -Body $loginBody `
        -ContentType "application/json" `
        -ErrorAction Stop
    
    $token = $loginResponse.token
    Write-Host "✓ Login successful" -ForegroundColor Green
    Write-Host "  Token: $($token.Substring(0, 20))..." -ForegroundColor Gray
}
catch {
    Write-Host "✗ Login failed: $_" -ForegroundColor Red
    Write-Host "  Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    exit 1
}

# Step 2: Get a subcategory ID (assuming subcategory 1 exists)
$subcategoryId = 1
Write-Host ""
Write-Host "[2/5] Using subcategory ID: $subcategoryId" -ForegroundColor Yellow

# Step 3: GET fields
Write-Host ""
Write-Host "[3/5] GET /api/admin/subcategories/$subcategoryId/fields" -ForegroundColor Yellow
try {
    $headers = @{
        Authorization = "Bearer $token"
    }
    
    $getResponse = Invoke-RestMethod -Uri "$BaseUrl/api/admin/subcategories/$subcategoryId/fields" `
        -Method GET `
        -Headers $headers `
        -ErrorAction Stop
    
    Write-Host "✓ GET fields successful" -ForegroundColor Green
    Write-Host "  Fields count: $($getResponse.Count)" -ForegroundColor Gray
    if ($getResponse.Count -gt 0) {
        Write-Host "  First field: $($getResponse[0].label) ($($getResponse[0].key))" -ForegroundColor Gray
    }
    $initialCount = $getResponse.Count
}
catch {
    Write-Host "✗ GET fields failed: $_" -ForegroundColor Red
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "  Status: $statusCode" -ForegroundColor Red
        
        try {
            $errorStream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($errorStream)
            $errorBody = $reader.ReadToEnd()
            Write-Host "  Response: $errorBody" -ForegroundColor Red
        }
        catch {}
    }
    exit 1
}

# Step 4: POST new field
Write-Host ""
Write-Host "[4/5] POST /api/admin/subcategories/$subcategoryId/fields" -ForegroundColor Yellow
$newField = @{
    name = "testField_$(Get-Date -Format 'yyyyMMddHHmmss')"
    label = "فیلد تست"
    key = "testField_$(Get-Date -Format 'yyyyMMddHHmmss')"
    type = "Text"
    isRequired = $false
    defaultValue = "test value"
} | ConvertTo-Json

try {
    $postResponse = Invoke-RestMethod -Uri "$BaseUrl/api/admin/subcategories/$subcategoryId/fields" `
        -Method POST `
        -Headers $headers `
        -Body $newField `
        -ContentType "application/json" `
        -ErrorAction Stop
    
    Write-Host "✓ POST field successful" -ForegroundColor Green
    Write-Host "  Created field ID: $($postResponse.id)" -ForegroundColor Gray
    Write-Host "  Field key: $($postResponse.key)" -ForegroundColor Gray
    $createdFieldId = $postResponse.id
}
catch {
    Write-Host "✗ POST field failed: $_" -ForegroundColor Red
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "  Status: $statusCode" -ForegroundColor Red
        
        try {
            $errorStream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($errorStream)
            $errorBody = $reader.ReadToEnd()
            Write-Host "  Response: $errorBody" -ForegroundColor Red
        }
        catch {}
    }
    exit 1
}

# Step 5: GET fields again to verify persistence
Write-Host ""
Write-Host "[5/5] GET /api/admin/subcategories/$subcategoryId/fields (verify persistence)" -ForegroundColor Yellow
try {
    $getResponse2 = Invoke-RestMethod -Uri "$BaseUrl/api/admin/subcategories/$subcategoryId/fields" `
        -Method GET `
        -Headers $headers `
        -ErrorAction Stop
    
    Write-Host "✓ GET fields successful (verification)" -ForegroundColor Green
    Write-Host "  Fields count: $($getResponse2.Count) (was $initialCount)" -ForegroundColor Gray
    
    $foundCreated = $getResponse2 | Where-Object { $_.id -eq $createdFieldId }
    if ($foundCreated) {
        Write-Host "✓ Created field persists in database" -ForegroundColor Green
    }
    else {
        Write-Host "✗ Created field not found in database" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "✗ GET fields (verification) failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Verification PASSED ===" -ForegroundColor Green
Write-Host ""
Write-Host "All field definitions endpoints are working correctly." -ForegroundColor Green
Write-Host ""

