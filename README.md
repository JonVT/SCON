# Stationeers SCON API Mod

<!-- VERSION_BADGE_START -->
![Version](https://img.shields.io/badge/version-1.0.1-blue.svg)
<!-- VERSION_BADGE_END -->

[![Build](https://github.com/JonVT/SCON/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/JonVT/SCON/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/JonVT/SCON)](https://github.com/JonVT/SCON/releases)

A BepInEx mod for Stationeers that exposes an HTTP API for executing console commands remotely.

Quick links:

- [Quickstart](./QUICKSTART.md) – Get up and running fast
- [Project Summary](./PROJECT_SUMMARY.md) – Overview, design, and scope

## Downloads

- Latest release: https://github.com/JonVT/SCON/releases/latest
- Windows package: download the asset named `SCON-<version>-windows.zip`
- Linux package: download the asset named `SCON-<version>-linux.zip`
- Advanced: you can also use the raw `SCON.dll` asset from the release

### Latest release via GitHub API (advanced)

- Endpoint: `https://api.github.com/repos/JonVT/SCON/releases/latest`

curl (Linux/macOS):
```bash
curl -H "Accept: application/vnd.github+json" \
  https://api.github.com/repos/JonVT/SCON/releases/latest

# Download Windows zip
curl -s -H "Accept: application/vnd.github+json" \
  https://api.github.com/repos/JonVT/SCON/releases/latest \
  | jq -r '.assets[] | select(.name|test("SCON-.*-windows\\.zip$")) | .browser_download_url' \
  | xargs -n1 -I{} curl -L -o SCON-latest-windows.zip {}

# Download Linux zip
curl -s -H "Accept: application/vnd.github+json" \
  https://api.github.com/repos/JonVT/SCON/releases/latest \
  | jq -r '.assets[] | select(.name|test("SCON-.*-linux\\.zip$")) | .browser_download_url' \
  | xargs -n1 -I{} curl -L -o SCON-latest-linux.zip {}
```

PowerShell (Windows):
```powershell
$headers = @{ "Accept" = "application/vnd.github+json" }
$rel = Invoke-RestMethod -Uri "https://api.github.com/repos/JonVT/SCON/releases/latest" -Headers $headers
$zipWin = $rel.assets | Where-Object { $_.name -like "SCON-*-windows.zip" } | Select-Object -First 1
if ($zipWin) { Invoke-WebRequest -Uri $zipWin.browser_download_url -OutFile $zipWin.name }

$zipLin = $rel.assets | Where-Object { $_.name -like "SCON-*-linux.zip" } | Select-Object -First 1
if ($zipLin) { Invoke-WebRequest -Uri $zipLin.browser_download_url -OutFile $zipLin.name }
```

## Features

- HTTP API server that listens for command requests
- Execute any Stationeers console command via API
- JSON-based request/response format
- Configurable port and host settings

## Installation

1. Install [BepInEx 5.x](https://github.com/BepInEx/BepInEx/releases) for Stationeers
  - Windows: use BepInEx x64 (Mono) and extract into the Stationeers game folder
  - Linux: use BepInEx 5.x x64 for Linux and follow the Linux-specific instructions on the release page
2. Download the latest SCON from the [Releases](https://github.com/JonVT/SCON/releases) page
  - Windows: download `SCON-<version>-windows.zip`
  - Linux: download `SCON-<version>-linux.zip`
  - Advanced: you can also use the raw `SCON.dll` asset if you prefer
3. Extract/copy `SCON.dll` into the `BepInEx/plugins/` folder
4. Launch Stationeers

Default game install locations:
- Windows (Steam): `C:\Program Files (x86)\Steam\steamapps\common\Stationeers`
- Linux (Steam): `$HOME/.local/share/Steam/steamapps/common/Stationeers`

## Configuration

The mod creates a configuration file at `BepInEx/config/SCON.cfg`:

```ini
[Server]
Port = 8080
Host = localhost
Enabled = true
ApiKey = 
AutoBindToServerPortPlusOne = true
```

### Authentication

- **ApiKey**: Optional API key for authentication
  - If **empty** (default): Localhost connections allowed without authentication, network connections denied
  - If **set**: All connections require the API key in the `Authorization` header
  
**Example with API key:**
```ini
ApiKey = your-secret-key-here
```

Then from any client:
```
Authorization: Bearer your-secret-key-here
```

### Dedicated server auto-binding

- When running a dedicated server (headless), SCON will automatically bind to `(game server port + 1)` if `AutoBindToServerPortPlusOne` is `true`.
- Example: If the game server runs on `27500`, SCON will bind to `27501`.
- Single-player or when the server port can't be detected: SCON uses the configured `Port`.

## API Usage

### Get Version

**Endpoint:** `GET /version`

**Response (JSON):**
```json
{
  "success": true,
  "name": "SCON",
  "guid": "com.stationeers.scon",
  "version": "<plugin-version>",
  "assemblyVersion": "<assembly-version>",
  "informationalVersion": "<informational-version>",
  "host": "localhost",
  "port": 8080
}
```

### Execute Command

**Endpoint:** `POST /command`

**Request Body (JSON):**
```json
{
  "command": "help"
}
```

**Response (JSON):**
```json
{
  "success": true,
  "message": "Command queued for execution"
}
```

### Get Game Information

**Endpoint:** `GET /gameinfo`

**Response (JSON):**
```json
{
  "success": true,
  "serverPort": 27500,
  "isServer": true,
  "worldName": "MyWorld",
  "sconVersion": "1.0.0",
  "sconHost": "localhost",
  "sconPort": 8080
}
```

Returns information about the running game instance including:
- `serverPort` - Game server port (if running as server)
- `isServer` - Whether instance is running as a server
- `worldName` - Current world/save name
- `sconVersion`, `sconHost`, `sconPort` - SCON server details

### Health Check

**Endpoint:** `GET /health`

**Response (JSON):**
```json
{
  "status": "ok"
}
```

### Example with curl:

```bash
# Health check
curl http://localhost:8080/health

# Get game info
curl http://localhost:8080/gameinfo

# Execute command without API key (localhost only)
curl -X POST http://localhost:8080/command \
  -H "Content-Type: application/json" \
  -d '{"command":"help"}'

# Execute command with API key
curl -X POST http://localhost:8080/command \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer your-secret-key-here" \
  -d '{"command":"help"}'
```

### Example with PowerShell:

```powershell
# Health check
Invoke-RestMethod -Uri "http://localhost:8080/health"

# Get game info
Invoke-RestMethod -Uri "http://localhost:8080/gameinfo"

# Execute command without API key (localhost only)
$body = @{
    command = "help"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:8080/command" -Method Post -Body $body -ContentType "application/json"

# Execute command with API key
$headers = @{
    Authorization = "Bearer your-secret-key-here"
}

Invoke-RestMethod -Uri "http://localhost:8080/command" -Method Post -Body $body -ContentType "application/json" -Headers $headers
```

## Supported Commands

Any console command that works in the Stationeers console will work through this API, including:
- `help` - List available commands
- `spawn [item]` - Spawn items
- `tp [x] [y] [z]` - Teleport
- `god` - Toggle god mode
- And many more...

## Building from Source

1. Set the `STATIONEERS_PATH` environment variable to your Stationeers installation directory
  - Windows (PowerShell): `$env:STATIONEERS_PATH = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Stationeers"`
  - Linux (bash): `export STATIONEERS_PATH="$HOME/.local/share/Steam/steamapps/common/Stationeers"`
2. Build
  - Windows: run `build.ps1` (optionally `-Install` to copy into BepInEx/plugins)
  - Linux/macOS: run `./build.sh` (optionally `--install`)
3. The compiled DLL will be in `bin/Release/net472/`

## Security Warning

⚠️ **Authentication:**
- By default (no API key): Only localhost can connect, network connections are denied
- With API key configured: All connections require the key in `Authorization` header
- **Recommended**: Set an API key if exposing to your network

⚠️ **Linux notes:**
- Binding to ports below 1024 requires elevated privileges.
- Wildcard hosts like `*` may not be supported in all environments; if binding fails, set `Host = localhost` (local only) or a specific interface IP.

## License

MIT License
