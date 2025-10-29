# Build script for StationeersRCON mod

param(
    [string]$StationeersPath = $env:STATIONEERS_PATH,
    [switch]$Install
)

Write-Host "Building StationeersRCON..." -ForegroundColor Cyan

# Check if Stationeers path is set
if ([string]::IsNullOrEmpty($StationeersPath)) {
    Write-Host "Warning: STATIONEERS_PATH environment variable not set." -ForegroundColor Yellow
    Write-Host "Set it with: `$env:STATIONEERS_PATH = 'C:\Path\To\Stationeers'" -ForegroundColor Yellow
    Write-Host ""
}

# Restore and build
Write-Host "Restoring NuGet packages..." -ForegroundColor Green
dotnet restore StationeersRCON.csproj

Write-Host "Building project..." -ForegroundColor Green
$buildResult = dotnet build StationeersRCON.csproj -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build successful!" -ForegroundColor Green

# Install if requested
if ($Install -and -not [string]::IsNullOrEmpty($StationeersPath)) {
    $pluginPath = Join-Path $StationeersPath "BepInEx\plugins"
    
    if (Test-Path $pluginPath) {
        Write-Host "Installing to $pluginPath..." -ForegroundColor Green
        Copy-Item "bin\Release\net472\StationeersRCON.dll" $pluginPath -Force
        Write-Host "Installation complete!" -ForegroundColor Green
    } else {
        Write-Host "BepInEx plugins folder not found. Is BepInEx installed?" -ForegroundColor Red
        Write-Host "Expected path: $pluginPath" -ForegroundColor Yellow
    }
} elseif ($Install) {
    Write-Host "Cannot install: STATIONEERS_PATH not set" -ForegroundColor Red
}

Write-Host ""
Write-Host "Output: bin\Release\net472\StationeersRCON.dll" -ForegroundColor Cyan
