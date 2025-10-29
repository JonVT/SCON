# Development Notes

## Project Structure

- **Plugin.cs** - Main BepInEx plugin entry point
- **PluginInfo.cs** - Plugin metadata (GUID, name, version)
- **HttpServer.cs** - HTTP server that listens for API requests
- **CommandExecutor.cs** - Executes commands via reflection into Stationeers console system

## How It Works

1. **BepInEx Plugin Loading**: The mod is loaded by BepInEx when Stationeers starts
2. **HTTP Server**: An HttpListener is started on the configured port (default 8080)
3. **API Endpoints**:
   - `POST /command` - Execute a console command
   - `GET /health` - Check if server is running
4. **Command Execution**: Commands are queued and executed on Unity's main thread using coroutines
5. **Console Integration**: Uses reflection to find and invoke Stationeers' console command system

## Configuration

Configuration file is automatically created at: `BepInEx/config/StationeersRCON.cfg`

```ini
[Server]
Enabled = true
Host = localhost
Port = 8080
```

## Console Integration Details

The `CommandExecutor` class uses reflection to locate the Stationeers console system:

1. Searches for console types (ConsoleWindow, DeveloperConsole, etc.)
2. Finds command execution methods (Submit, ExecuteCommand, RunCommand)
3. Locates console instance (singleton pattern or FindObjectOfType)
4. Invokes the command execution method

If the main method fails, it tries alternative approaches:
- GameObject.Find + SendMessage
- Direct command parsing (future enhancement)

## Security Considerations

⚠️ **Important**: This mod allows remote command execution!

- Default configuration binds to `localhost` only
- To expose to network, change Host to `*` or `0.0.0.0`
- **Highly recommended**: Add authentication for network exposure
- Consider firewall rules to restrict access

## Future Enhancements

- [ ] Add authentication (API key, bearer token)
- [ ] Support for command output capture
- [ ] WebSocket support for real-time events
- [ ] Command history and logging
- [ ] Rate limiting
- [ ] SSL/TLS support

## Testing

1. Start Stationeers with the mod installed
2. Check BepInEx console for "RCON server started" message
3. Test health endpoint:
   ```powershell
   Invoke-RestMethod -Uri "http://localhost:8080/health"
   ```
4. Execute a command:
   ```powershell
   $body = @{ command = "help" } | ConvertTo-Json
   Invoke-RestMethod -Uri "http://localhost:8080/command" -Method Post -Body $body -ContentType "application/json"
   ```

## Debugging

- Enable BepInEx console to see detailed logs
- Check `BepInEx/LogOutput.log` for errors
- Use `CommandExecutor` logging to debug command execution
- Test with simple commands first (like "help")

## Building

### Prerequisites
- .NET SDK 6.0 or later
- Stationeers game files
- BepInEx 5.x

### Environment Setup
```powershell
$env:STATIONEERS_PATH = "C:\Program Files (x86)\Steam\steamapps\common\Stationeers"
```

### Build Commands
```powershell
# Restore packages
dotnet restore

# Build
dotnet build -c Release

# Build and install
.\build.ps1 -Install
```

## Troubleshooting

### "Assembly-CSharp.dll not found"
- Set STATIONEERS_PATH environment variable
- Ensure path points to Stationeers installation directory

### "Console type not found"
- Stationeers may have changed console implementation
- Check BepInEx logs for discovered types
- Update CommandExecutor with correct type names

### "HTTP listener failed to start"
- Port may be in use
- Requires admin privileges for non-localhost bindings on Windows
- Check firewall settings

### Commands not executing
- Check BepInEx console for errors
- Verify console is available in game
- Try simple commands first
- Enable verbose logging in CommandExecutor
