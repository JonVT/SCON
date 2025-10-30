# Stationeers SCON API Mod

A BepInEx mod for Stationeers that exposes an HTTP API for executing console commands remotely.

Quick links:

- [Quickstart](./QUICKSTART.md) – Get up and running fast
- [Project Summary](./PROJECT_SUMMARY.md) – Overview, design, and scope

## Features

- HTTP API server that listens for command requests
- Execute any Stationeers console command via API
- JSON-based request/response format
- Configurable port and host settings

## Installation

1. Install [BepInEx 5.x](https://github.com/BepInEx/BepInEx/releases) for Stationeers
  - Windows: use BepInEx x64 (Mono) and extract into the Stationeers game folder
  - Linux: use BepInEx 5.x x64 for Linux and follow the Linux-specific instructions on the release page
2. Download the latest release of SCON
3. Extract `SCON.dll` to `BepInEx/plugins/` folder
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
  "rconVersion": "1.0.0",
  "rconHost": "localhost",
  "rconPort": 8080
}
```

Returns information about the running game instance including:
- `serverPort` - Game server port (if running as server)
- `isServer` - Whether instance is running as a server
- `worldName` - Current world/save name
- `rconVersion`, `rconHost`, `rconPort` - SCON server details

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
