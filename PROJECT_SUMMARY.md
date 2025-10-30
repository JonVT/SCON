# SCON - Project Summary

## What This Mod Does

SCON is a BepInEx mod for Stationeers that exposes an HTTP REST API, allowing you to send console commands to the game from external applications. This enables automation, remote control, and integration with other tools.

## Key Features

✅ **HTTP REST API** - Simple JSON-based API for sending commands
✅ **Console Command Execution** - Execute any Stationeers console command remotely  
✅ **Configurable** - Customize host, port, and enable/disable via config file
✅ **Cross-platform Client Support** - Use from PowerShell, Python, JavaScript, curl, etc.
✅ **CORS Enabled** - Can be called from web applications
✅ **Health Check Endpoint** - Monitor if the server is running
✅ **Background Operation** - Runs silently without interrupting gameplay

## Project Structure

```
SCON/
├── Source Code
│   ├── Plugin.cs              # BepInEx plugin entry point
│   ├── PluginInfo.cs          # Plugin metadata
│   ├── HttpServer.cs          # HTTP server implementation
│   └── CommandExecutor.cs     # Console command execution via reflection
│
├── Documentation
│   ├── README.md              # Main documentation & API reference
│   ├── QUICKSTART.md          # Installation & getting started guide
│   ├── DEVELOPMENT.md         # Developer notes & technical details
│   └── PROJECT_SUMMARY.md     # This file
│
├── Scripts
│   ├── build.ps1              # Build script with optional install
│   ├── test.ps1               # API test suite
│   └── examples.ps1           # Usage examples
│
├── Configuration
│   ├── SCON.csproj # Project file
│   ├── .gitignore             # Git ignore rules
│   └── .vscode/               # VS Code tasks & settings
│
└── Output
    └── bin/Release/net472/SCON.dll  # Compiled mod
```

## Technical Architecture

### Components

1. **BepInEx Plugin (Plugin.cs)**
   - Initializes when Stationeers starts
   - Loads configuration
   - Creates and manages HTTP server component

2. **HTTP Server (HttpServer.cs)**
   - HttpListener on separate thread
   - Handles POST /command and GET /health endpoints
   - Queues commands to Unity main thread via coroutines
   - Returns JSON responses

3. **Command Executor (CommandExecutor.cs)**
   - Uses reflection to find Stationeers console system
   - Locates console instance (singleton or FindObjectOfType)
   - Invokes command execution method
   - Falls back to alternative methods if primary fails

### Communication Flow

```
External Client (curl/Python/etc)
    ↓ HTTP POST
HttpServer (Background Thread)
    ↓ Queue Command
Unity Coroutine (Main Thread)
    ↓ Reflection
Stationeers Console System
    ↓ Execute
Game Engine
```

## API Reference

### POST /command
Execute a console command

**Request:**
```json
{
  "command": "help"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Command queued for execution"
}
```

### GET /health
Check server status

**Response:**
```json
{
  "status": "ok"
}
```

## Usage Examples

### PowerShell
```powershell
$body = @{ command = "god" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:8080/command" -Method Post -Body $body -ContentType "application/json"
```

### Python
```python
import requests
requests.post("http://localhost:8080/command", json={"command": "help"})
```

### curl
```bash
curl -X POST http://localhost:8080/command \
  -H "Content-Type: application/json" \
  -d '{"command":"help"}'
```

## Building & Installation

### Quick Build

Windows (PowerShell):
```powershell
# Set Stationeers path
$env:STATIONEERS_PATH = "C:\\...\\Stationeers"

# Build and install
./build.ps1 -Install
```

Linux (bash):
```bash
export STATIONEERS_PATH="$HOME/.local/share/Steam/steamapps/common/Stationeers"
./build.sh --install
```

### Manual Steps
1. `dotnet restore`
2. `dotnet build -c Release`
3. Copy the built DLL to the plugins folder:
  - Windows: `bin\Release\net472\SCON.dll` -> `Stationeers\BepInEx\plugins\`
  - Linux: `bin/Release/net472/SCON.dll` -> `$STATIONEERS_PATH/BepInEx/plugins/`

## Configuration

File: `Stationeers\BepInEx\config\SCON.cfg`

```ini
[Server]
Enabled = true      # Enable/disable server
Host = localhost    # Bind address (* = all interfaces)
Port = 8080         # Port number
```

## Security Notes

⚠️ **Important Considerations:**

- **Default**: Binds to `localhost` only (safe)
- **Network**: Setting `Host = *` exposes to network
- **No Authentication**: Currently no built-in auth
- **Full Access**: Can execute ANY console command
- **Recommendation**: Use on trusted networks only or add authentication layer

## Testing

```powershell
# Run automated tests
.\test.ps1

# Run interactive examples
.\examples.ps1
```

## Common Use Cases

1. **Automation Scripts** - Schedule tasks or automate repetitive actions
2. **External Tools** - Build GUIs or web interfaces for server management
3. **Monitoring** - Track game state and respond to events
4. **Development** - Quickly test commands during mod development
5. **Server Management** - Remote administration of dedicated servers

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Server not starting | Check BepInEx console, verify port not in use |
| Commands not executing | Ensure in-game (not menu), check console access |
| Build fails | Set STATIONEERS_PATH, verify game files |
| Network access fails | Requires admin for network binding on Windows |

## Future Enhancement Ideas

- Authentication (API keys, tokens)
- Command output capture and return
- WebSocket support for real-time events
- Command history and logging
- Rate limiting
- SSL/TLS support
- Multiple concurrent sessions

## Dependencies

- **BepInEx 5.x** - Modding framework (Windows & Linux)
- **.NET Framework 4.7.2/Mono** - Runtime (via Unity/Mono and BepInEx on Linux)
- **UnityEngine 2020.3.26** - Game engine APIs
- **Stationeers Assembly-CSharp** - Game code access

## License

MIT License - Free to use and modify

## Version History

- **v1.0.0** - Initial release
  - HTTP API server
  - Console command execution
  - Configuration support
  - CORS enabled
  - Health check endpoint

---

**Quick Links:**
- 📖 [Full Documentation](README.md)
- 🚀 [Quick Start Guide](QUICKSTART.md)
- 🔧 [Developer Notes](DEVELOPMENT.md)
- 💻 [Usage Examples](examples.ps1)
- ✅ [Test Suite](test.ps1)
 - 🐧 Linux Build Script (bash): [build.sh](build.sh)
