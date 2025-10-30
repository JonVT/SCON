# Quick Start Guide - SCON

## Prerequisites

1. **Stationeers** game installed
2. **BepInEx 5.x** installed for Stationeers
   - Download from: https://github.com/BepInEx/BepInEx/releases
   - Extract to Stationeers game folder
   - Run game once to generate BepInEx folders
3. **.NET SDK 6.0+** (for building from source)

## Option 1: Install Pre-built Mod (Recommended)

1. Download `SCON.dll` from releases
2. Copy to `Stationeers/BepInEx/plugins/`
3. Launch Stationeers
4. Check BepInEx console for "SCON server started" message

## Option 2: Build from Source

### Step 1: Set Environment Variable

```powershell
# Set your Stationeers installation path
$env:STATIONEERS_PATH = "C:\Program Files (x86)\Steam\steamapps\common\Stationeers"
```

Or set permanently:
```powershell
[System.Environment]::SetEnvironmentVariable('STATIONEERS_PATH', 'C:\...\Stationeers', 'User')
```

### Step 2: Build

```powershell
# Navigate to project folder
cd "e:\Users\jon\OneDrive\Projects\Stationeers\SCON"

# Build the project
.\build.ps1

# Or build and auto-install
.\build.ps1 -Install
```

### Step 3: Manual Install (if not using -Install flag)

Copy `bin\Release\net472\SCON.dll` to `Stationeers\BepInEx\plugins\`

## First Run

1. **Start Stationeers**
2. **Check BepInEx Console** (press F5 if not visible)
    - Look for: `SCON server started on localhost:8080`
3. **Test the API**:

```powershell
# Test health endpoint
Invoke-RestMethod -Uri "http://localhost:8080/health"

# Execute a command
$body = @{ command = "help" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:8080/command" -Method Post -Body $body -ContentType "application/json"
```

## Configuration

Config file location: `Stationeers\BepInEx\config\SCON.cfg`

```ini
[Server]
## Enable or disable the RCON server
# Setting type: Boolean
# Default value: true
Enabled = true

## Host address to bind the server to (use * for all interfaces)
# Setting type: String
# Default value: localhost
Host = localhost

## Port number for the RCON server
# Setting type: Int32
# Default value: 8080
Port = 8080
```

**Important**: Restart Stationeers after changing configuration.

## Usage Examples

### PowerShell Function

```powershell
function Send-StationeersCommand {
    param([string]$Command)
    
    $body = @{ command = $Command } | ConvertTo-Json
    Invoke-RestMethod -Uri "http://localhost:8080/command" `
        -Method Post `
        -Body $body `
        -ContentType "application/json"
}

# Use it
Send-StationeersCommand "help"
Send-StationeersCommand "god"
Send-StationeersCommand "spawn ItemKitSuitSpace"
```

### Using Provided Scripts

```powershell
# Run examples
.\examples.ps1

# Run tests
.\test.ps1
```

### Python Example

```python
import requests
import json

def send_command(command):
    url = "http://localhost:8080/command"
    payload = {"command": command}
    response = requests.post(url, json=payload)
    return response.json()

# Execute command
result = send_command("help")
print(result)
```

### JavaScript/Node.js Example

```javascript
const fetch = require('node-fetch');

async function sendCommand(command) {
    const response = await fetch('http://localhost:8080/command', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ command })
    });
    return await response.json();
}

// Execute command
sendCommand('help').then(console.log);
```

## Troubleshooting

### "SCON server not responding"

1. Check if mod is loaded in BepInEx console
2. Verify port is not in use: `netstat -an | Select-String 8080`
3. Check firewall settings
4. Review `BepInEx\LogOutput.log` for errors

### "Commands not executing"

1. Ensure you're in-game (not main menu)
2. Try simple commands first: `help`, `god`
3. Check BepInEx console for execution logs
4. Verify console is accessible in game (F1 key)

### "Build failed - Assembly-CSharp not found"

1. Verify STATIONEERS_PATH is set correctly
2. Check path points to game root (contains `rocketstation.exe`)
3. Verify `rocketstation_Data\Managed\Assembly-CSharp.dll` exists

### "Permission denied on port"

- Default `localhost:8080` should work without admin
- Network binding (`Host = *`) requires admin on Windows
- Try changing port in config

## Next Steps

- Read `DEVELOPMENT.md` for technical details
- Review API documentation in `README.md`
- Create automation scripts for common tasks
- Consider adding authentication for network use

## Support

For issues or questions:
1. Check BepInEx logs: `BepInEx\LogOutput.log`
2. Enable BepInEx console for real-time logs
3. Review `DEVELOPMENT.md` for debugging tips
