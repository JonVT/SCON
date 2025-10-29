# Stationeers RCON API Mod

A BepInEx mod for Stationeers that exposes an HTTP API for executing console commands remotely.

## Features

- HTTP API server that listens for command requests
- Execute any Stationeers console command via API
- JSON-based request/response format
- Configurable port and host settings

## Installation

1. Install [BepInEx 5.x](https://github.com/BepInEx/BepInEx/releases) for Stationeers
2. Download the latest release of StationeersRCON
3. Extract `StationeersRCON.dll` to `BepInEx/plugins/` folder
4. Launch Stationeers

## Configuration

The mod creates a configuration file at `BepInEx/config/StationeersRCON.cfg`:

```ini
[Server]
Port = 8080
Host = localhost
Enabled = true
```

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
  "message": "Command executed successfully"
}
```

### Example with curl:

```bash
curl -X POST http://localhost:8080/command \
  -H "Content-Type: application/json" \
  -d '{"command":"help"}'
```

### Example with PowerShell:

```powershell
$body = @{
    command = "help"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:8080/command" -Method Post -Body $body -ContentType "application/json"
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
2. Run `dotnet build`
3. The compiled DLL will be in `bin/Debug/net472/`

## Security Warning

⚠️ This mod allows remote command execution. Only use it on trusted networks or localhost. Consider implementing authentication if exposing to a network.

## License

MIT License
