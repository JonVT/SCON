# Quick Start Guide - SCON

## Prerequisites

1. Stationeers installed
2. BepInEx 5.x installed for Stationeers
   - Download: https://github.com/BepInEx/BepInEx/releases
   - Windows: extract to the Stationeers folder; run game once to generate folders
   - Linux: use the Linux x64 build; extract to the Stationeers folder; run once to generate folders
3. .NET SDK 6.0+ (for building from source)

## Option 1: Install Pre-built Mod (Recommended)

1. Download `SCON.dll` from releases
2. Copy to `Stationeers/BepInEx/plugins/`
3. Launch Stationeers
4. Check BepInEx console for "SCON server started" message

## Option 2: Build from Source (Windows & Linux)

### Step 1: Set STATIONEERS_PATH

Windows (PowerShell):
```powershell
$env:STATIONEERS_PATH = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Stationeers"
[System.Environment]::SetEnvironmentVariable('STATIONEERS_PATH', 'C:\\...\\Stationeers', 'User') # optional
```

Linux (bash):
```bash
export STATIONEERS_PATH="$HOME/.local/share/Steam/steamapps/common/Stationeers"
```

### Step 2: Build

Windows (PowerShell):
```powershell
cd "e:\\Users\\jon\\OneDrive\\Projects\\Stationeers\\SCON"
./build.ps1          # or: ./build.ps1 -Install
```

Linux (bash):
```bash
cd "$HOME/path/to/SCON"
chmod +x build.sh
./build.sh           # or: ./build.sh --install
```

### Step 3: Manual Install (if not using auto-install)

- Windows: copy `bin\Release\net472\SCON.dll` → `Stationeers\BepInEx\plugins\`
- Linux: copy `bin/Release/net472/SCON.dll` → `$STATIONEERS_PATH/BepInEx/plugins/`

## First Run

1. Start Stationeers
2. Check BepInEx Console (F5) for: `SCON server started on ...`
3. Test the API:

PowerShell:
```powershell
Invoke-RestMethod -Uri "http://localhost:8080/health"
Invoke-RestMethod -Uri "http://localhost:8080/version"
$body = @{ command = "help" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:8080/command" -Method Post -Body $body -ContentType "application/json"
```

curl:
```bash
curl http://localhost:8080/health
curl http://localhost:8080/version
curl -X POST http://localhost:8080/command -H "Content-Type: application/json" -d '{"command":"help"}'
```

## Configuration

File: `Stationeers/BepInEx/config/SCON.cfg`

```ini
[Server]
Enabled = true
Host = localhost
Port = 8080
ApiKey = 
AutoBindToServerPortPlusOne = true
```

Note: Restart Stationeers after changing config.

## Usage Examples

PowerShell helper:
```powershell
function Send-StationeersCommand {
  param([string]$Command)
  $body = @{ command = $Command } | ConvertTo-Json
  Invoke-RestMethod -Uri "http://localhost:8080/command" -Method Post -Body $body -ContentType "application/json"
}

Send-StationeersCommand "help"
```

Python:
```python
import requests

def send_command(cmd):
    return requests.post("http://localhost:8080/command", json={"command": cmd}).json()

print(send_command("help"))
```

Node.js:
```javascript
const fetch = require('node-fetch');
(async () => {
  const r = await fetch('http://localhost:8080/command', {
    method: 'POST', headers: {'Content-Type': 'application/json'}, body: JSON.stringify({command: 'help'})
  });
  console.log(await r.json());
})();
```

## Troubleshooting

SCON server not responding:
1) Check BepInEx console for errors
2) Port in use? Windows: `netstat -an | Select-String 8080`; Linux: `ss -ltnp | grep 8080` or `lsof -i :8080`
3) Firewall rules
4) Check `BepInEx/LogOutput.log`

Build failed - Assembly-CSharp not found:
1) Verify STATIONEERS_PATH
2) Path points to game root (Windows: `rocketstation.exe`; Linux: `rocketstation.x86_64`)
3) `rocketstation_Data/Managed/Assembly-CSharp.dll` exists

Permission denied on port:
- `localhost:8080` should work without admin
- Windows network binding (`Host = *`) may require admin; Linux ports <1024 need elevated privileges
- Change port in config if needed

## Next Steps

- Read `DEVELOPMENT.md`
- Review API in `README.md`
- Add auth if exposing beyond localhost

## Support

1) Check BepInEx logs: `BepInEx/LogOutput.log`
2) Enable BepInEx console
3) See `DEVELOPMENT.md`
