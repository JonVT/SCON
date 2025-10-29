# Example API usage script for StationeersRCON

$baseUrl = "http://localhost:8080"

function Invoke-StationeersCommand {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Command,
        [string]$ServerUrl = $baseUrl
    )
    
    $body = @{
        command = $Command
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "$ServerUrl/command" `
            -Method Post `
            -Body $body `
            -ContentType "application/json"
        
        Write-Host "✓ Command sent: $Command" -ForegroundColor Green
        return $response
    }
    catch {
        Write-Host "✗ Error: $_" -ForegroundColor Red
        return $null
    }
}

function Test-RCONHealth {
    param([string]$ServerUrl = $baseUrl)
    
    try {
        $response = Invoke-RestMethod -Uri "$ServerUrl/health" -Method Get
        Write-Host "✓ RCON server is healthy" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "✗ RCON server is not responding" -ForegroundColor Red
        return $false
    }
}

# Example usage
Write-Host "StationeersRCON API Examples" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan
Write-Host ""

# Check health
Write-Host "Checking server health..." -ForegroundColor Yellow
Test-RCONHealth
Write-Host ""

# Example commands
Write-Host "Example commands:" -ForegroundColor Yellow
Write-Host ""

# Show help
Write-Host "1. Show help:" -ForegroundColor Cyan
Invoke-StationeersCommand -Command "help"
Start-Sleep -Seconds 1

# Toggle god mode
Write-Host "`n2. Toggle god mode:" -ForegroundColor Cyan
Invoke-StationeersCommand -Command "god"
Start-Sleep -Seconds 1

# Teleport example (adjust coordinates as needed)
Write-Host "`n3. Teleport to coordinates:" -ForegroundColor Cyan
Invoke-StationeersCommand -Command "tp 0 0 0"
Start-Sleep -Seconds 1

# Spawn item example
Write-Host "`n4. Spawn an item:" -ForegroundColor Cyan
Invoke-StationeersCommand -Command "spawn ItemKitSuitSpace"
Start-Sleep -Seconds 1

# Weather command
Write-Host "`n5. Change weather:" -ForegroundColor Cyan
Invoke-StationeersCommand -Command "weather clear"

Write-Host "`n=============================" -ForegroundColor Cyan
Write-Host "Examples complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Create your own automation:" -ForegroundColor Yellow
Write-Host '  Invoke-StationeersCommand -Command "your command here"' -ForegroundColor Gray
