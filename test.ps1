# Test script for SCON API

param(
    [string]$ServerUrl = "http://localhost:8080"
)

Write-Host "SCON Test Suite" -ForegroundColor Cyan
Write-Host "==========================" -ForegroundColor Cyan
Write-Host ""

$passed = 0
$failed = 0

function Test-Endpoint {
    param(
        [string]$Name,
        [scriptblock]$TestBlock
    )
    
    Write-Host "Testing: $Name" -ForegroundColor Yellow
    try {
        $result = & $TestBlock
        if ($result) {
            Write-Host "  ✓ PASSED" -ForegroundColor Green
            $script:passed++
        } else {
            Write-Host "  ✗ FAILED" -ForegroundColor Red
            $script:failed++
        }
    }
    catch {
        Write-Host "  ✗ FAILED: $_" -ForegroundColor Red
        $script:failed++
    }
    Write-Host ""
}

# Test 1: Health endpoint
Test-Endpoint "Health Endpoint (GET /health)" {
    $response = Invoke-RestMethod -Uri "$ServerUrl/health" -Method Get -ErrorAction Stop
    return $response.status -eq "ok"
}

# Test 2: Command endpoint with valid command
Test-Endpoint "Command Endpoint (POST /command)" {
    $body = @{ command = "help" } | ConvertTo-Json
    $response = Invoke-RestMethod -Uri "$ServerUrl/command" `
        -Method Post `
        -Body $body `
        -ContentType "application/json" `
        -ErrorAction Stop
    return $response.success -eq $true
}

# Test 3: Invalid endpoint
Test-Endpoint "Invalid Endpoint Returns 404" {
    try {
        Invoke-RestMethod -Uri "$ServerUrl/invalid" -Method Get -ErrorAction Stop
        return $false
    }
    catch {
        return $_.Exception.Response.StatusCode -eq 404
    }
}

# Test 4: Empty command
Test-Endpoint "Empty Command Handling" {
    $body = @{ command = "" } | ConvertTo-Json
    $response = Invoke-RestMethod -Uri "$ServerUrl/command" `
        -Method Post `
        -Body $body `
        -ContentType "application/json" `
        -ErrorAction Stop
    return $response.success -eq $false
}

# Test 5: CORS headers
Test-Endpoint "CORS Headers Present" {
    $response = Invoke-WebRequest -Uri "$ServerUrl/health" -Method Options -ErrorAction Stop
    $headers = $response.Headers
    return $headers.ContainsKey("Access-Control-Allow-Origin")
}

# Test 6: JSON content type
Test-Endpoint "Response Content-Type is JSON" {
    $response = Invoke-WebRequest -Uri "$ServerUrl/health" -Method Get -ErrorAction Stop
    return $response.Headers["Content-Type"] -match "application/json"
}

# Summary
Write-Host "==========================" -ForegroundColor Cyan
Write-Host "Test Results:" -ForegroundColor Cyan
Write-Host "  Passed: $passed" -ForegroundColor Green
Write-Host "  Failed: $failed" -ForegroundColor $(if ($failed -gt 0) { "Red" } else { "Green" })
Write-Host ""

if ($failed -eq 0) {
    Write-Host "✓ All tests passed!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "✗ Some tests failed" -ForegroundColor Red
    exit 1
}
