# Build script for SCON mod

param(
    [string]$StationeersPath = $env:STATIONEERS_PATH,
    [switch]$Install
)

Write-Host "Building SCON..." -ForegroundColor Cyan

# Check if Stationeers path is set
if ([string]::IsNullOrEmpty($StationeersPath)) {
    Write-Host "Warning: STATIONEERS_PATH environment variable not set." -ForegroundColor Yellow
    Write-Host "Windows (PowerShell): `$env:STATIONEERS_PATH = 'C:\\Path\\To\\Stationeers'" -ForegroundColor Yellow
    Write-Host "Linux (bash): export STATIONEERS_PATH=\"$HOME/.local/share/Steam/steamapps/common/Stationeers\"" -ForegroundColor Yellow
    Write-Host ""
}

# Restore and build
Write-Host "Restoring NuGet packages..." -ForegroundColor Green
dotnet restore SCON.csproj

Write-Host "Building project..." -ForegroundColor Green
$buildResult = dotnet build SCON.csproj -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build successful!" -ForegroundColor Green

# Install if requested
if ($Install -and -not [string]::IsNullOrEmpty($StationeersPath)) {
    # Cross-platform safe path combining
    $bepPath = Join-Path $StationeersPath "BepInEx"
    $pluginPath = Join-Path $bepPath "plugins"

    if (Test-Path $pluginPath) {
        Write-Host "Installing to $pluginPath..." -ForegroundColor Green
        $outPath = Join-Path (Join-Path (Join-Path "bin" "Release") "net472") "SCON.dll"
        Copy-Item $outPath $pluginPath -Force
        Write-Host "Installation complete!" -ForegroundColor Green
    } else {
        Write-Host "BepInEx plugins folder not found. Is BepInEx installed?" -ForegroundColor Red
        Write-Host "Expected path: $pluginPath" -ForegroundColor Yellow
    }
} elseif ($Install) {
    Write-Host "Cannot install: STATIONEERS_PATH not set" -ForegroundColor Red
}

Write-Host ""
Write-Host "Output: $(Join-Path (Join-Path (Join-Path "bin" "Release") "net472") "SCON.dll")" -ForegroundColor Cyan
