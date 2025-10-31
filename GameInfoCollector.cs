using System;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SCON
{
    public static class GameInfoCollector
    {
        public static bool TryGetServerPort(out int port, out bool isDedicated)
        {
            port = 0;
            isDedicated = false;
            try
            {
                var assemblyCSharp = Assembly.Load("Assembly-CSharp");
                Type networkManagerType = null;
                foreach (var type in assemblyCSharp.GetTypes())
                {
                    if (type.Name.Contains("NetworkManager") && !type.Name.Contains("<"))
                    {
                        networkManagerType = type;
                        break;
                    }
                }

                bool isServerFlag = false;
                if (networkManagerType != null)
                {
                    var instanceProp = networkManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    var instanceField = networkManagerType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
                    object networkManager = instanceProp?.GetValue(null) ?? instanceField?.GetValue(null);

                    if (networkManager != null)
                    {
                        // Fields containing a port
                        foreach (var field in networkManagerType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                        {
                            if (field.Name.ToLower().Contains("port") && (field.FieldType == typeof(int) || field.FieldType == typeof(ushort)))
                            {
                                var value = field.GetValue(networkManager);
                                if (value != null)
                                {
                                    int p = value is ushort us ? us : (int)value;
                                    if (p > 0 && p < 65536)
                                    {
                                        port = p;
                                        break;
                                    }
                                }
                            }
                        }

                        // Properties containing a port
                        if (port == 0)
                        {
                            foreach (var prop in networkManagerType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                            {
                                if (!prop.CanRead) continue;
                                if (prop.Name.ToLower().Contains("port") && (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(ushort)))
                                {
                                    object value = null;
                                    try { value = prop.GetValue(networkManager); } catch { }
                                    if (value != null)
                                    {
                                        int p = value is ushort us ? us : (int)value;
                                        if (p > 0 && p < 65536)
                                        {
                                            port = p;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        // server flag
                        foreach (var prop in networkManagerType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            if (prop.Name.ToLower().Contains("server") && prop.PropertyType == typeof(bool))
                            {
                                var value = prop.GetValue(networkManager);
                                if (value is bool b)
                                {
                                    isServerFlag = isServerFlag || b;
                                }
                            }
                        }
                    }
                }

                // Dedicated server if running headless or server flag true
                isDedicated = Application.isBatchMode || isServerFlag;

                // As a final fallback, parse common command-line args for server port
                if (port == 0)
                {
                    try
                    {
                        var args = Environment.GetCommandLineArgs();
                        for (int i = 0; i < args.Length; i++)
                        {
                            var a = args[i];
                            if (string.Equals(a, "-port", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(a, "--port", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(a, "-serverPort", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(a, "--serverPort", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(a, "-serverport", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(a, "--serverport", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(a, "GamePort", StringComparison.OrdinalIgnoreCase))
                            {
                                if (i + 1 < args.Length && int.TryParse(args[i + 1], out var cliPort))
                                {
                                    if (cliPort > 0 && cliPort < 65536)
                                    {
                                        port = cliPort;
                                        break;
                                    }
                                }
                            }
                            // handle forms like --port=12345
                            if (a.StartsWith("-port=", StringComparison.OrdinalIgnoreCase) || a.StartsWith("--port=", StringComparison.OrdinalIgnoreCase)
                                || a.StartsWith("-serverPort=", StringComparison.OrdinalIgnoreCase) || a.StartsWith("--serverPort=", StringComparison.OrdinalIgnoreCase)
                                || a.StartsWith("-serverport=", StringComparison.OrdinalIgnoreCase) || a.StartsWith("--serverport=", StringComparison.OrdinalIgnoreCase)
                                || a.StartsWith("GamePort=", StringComparison.OrdinalIgnoreCase))
                            {
                                var idx = a.IndexOf('=');
                                if (idx > 0 && int.TryParse(a.Substring(idx + 1), out var cliPort2))
                                {
                                    if (cliPort2 > 0 && cliPort2 < 65536)
                                    {
                                        port = cliPort2;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
                return port != 0;
            }
            catch
            {
                return false;
            }
        }

        public static string GetGameInfo()
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"success\":true,");
            
            try
            {
                // Load Assembly-CSharp for reflection
                var assemblyCSharp = Assembly.Load("Assembly-CSharp");

                // Determine server port and dedicated state via helper (robust across versions)
                int detectedPort;
                bool isDedicated;
                if (TryGetServerPort(out detectedPort, out isDedicated) && detectedPort > 0)
                {
                    sb.Append($"\"serverPort\":{detectedPort},");
                }

                // Try to get server flag from NetworkManager if available
                bool? nmIsServer = null;
                var networkManagerType = assemblyCSharp.GetType("Assets.Scripts.Networking.NetworkManager")
                    ?? assemblyCSharp.GetType("NetworkManager");
                if (networkManagerType != null)
                {
                    var instanceProp = networkManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    var instanceField = networkManagerType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
                    object networkManager = instanceProp?.GetValue(null) ?? instanceField?.GetValue(null);
                    if (networkManager != null)
                    {
                        var isServerProp = networkManagerType.GetProperty("IsServer", BindingFlags.Public | BindingFlags.Instance)
                            ?? networkManagerType.GetProperty("Server", BindingFlags.Public | BindingFlags.Instance)
                            ?? networkManagerType.GetProperty("IsDedicatedServer", BindingFlags.Public | BindingFlags.Instance);
                        var isServerField = networkManagerType.GetField("IsServer", BindingFlags.Public | BindingFlags.Instance)
                            ?? networkManagerType.GetField("isServer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            ?? networkManagerType.GetField("_isServer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            ?? networkManagerType.GetField("IsDedicatedServer", BindingFlags.Public | BindingFlags.Instance);

                        if (isServerProp != null)
                        {
                            nmIsServer = isServerProp.GetValue(networkManager) as bool?;
                        }
                        else if (isServerField != null)
                        {
                            var v = isServerField.GetValue(networkManager);
                            if (v is bool b) nmIsServer = b;
                        }
                    }
                }

                // Compute final isServer with robust fallbacks
                bool isServerFinal = (nmIsServer ?? false) || isDedicated || Application.isBatchMode;
                sb.Append($"\"isServer\":{isServerFinal.ToString().ToLower()},");

                // Try to get world/save name
                var worldManagerType = assemblyCSharp.GetType("Assets.Scripts.WorldManager")
                    ?? assemblyCSharp.GetType("WorldManager");
                
                if (worldManagerType != null)
                {
                    var instanceProp = worldManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    var instanceField = worldManagerType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
                    
                    object worldManager = instanceProp?.GetValue(null) ?? instanceField?.GetValue(null);
                    
                    if (worldManager != null)
                    {
                        var worldNameProp = worldManagerType.GetProperty("WorldName", BindingFlags.Public | BindingFlags.Instance)
                            ?? worldManagerType.GetProperty("SaveName", BindingFlags.Public | BindingFlags.Instance)
                            ?? worldManagerType.GetProperty("CurrentSaveName", BindingFlags.Public | BindingFlags.Instance);
                        var worldNameField = worldManagerType.GetField("WorldName", BindingFlags.Public | BindingFlags.Instance)
                            ?? worldManagerType.GetField("worldName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            ?? worldManagerType.GetField("SaveName", BindingFlags.Public | BindingFlags.Instance)
                            ?? worldManagerType.GetField("currentSaveName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        string worldName = null;
                        if (worldNameProp != null)
                        {
                            try { worldName = worldNameProp.GetValue(worldManager) as string; } catch { }
                        }
                        if (string.IsNullOrEmpty(worldName) && worldNameField != null)
                        {
                            try { worldName = worldNameField.GetValue(worldManager) as string; } catch { }
                        }

                        // Fallback: try a likely nested CurrentWorld.Name pattern
                        if (string.IsNullOrEmpty(worldName))
                        {
                            var currentWorldProp = worldManagerType.GetProperty("CurrentWorld", BindingFlags.Public | BindingFlags.Instance)
                                ?? worldManagerType.GetProperty("World", BindingFlags.Public | BindingFlags.Instance);
                            object currentWorld = null;
                            try { currentWorld = currentWorldProp?.GetValue(worldManager); } catch { }
                            if (currentWorld != null)
                            {
                                var cwType = currentWorld.GetType();
                                var nameProp = cwType.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance)
                                    ?? cwType.GetProperty("WorldName", BindingFlags.Public | BindingFlags.Instance);
                                if (nameProp != null)
                                {
                                    try { worldName = nameProp.GetValue(currentWorld) as string; } catch { }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(worldName))
                        {
                            sb.Append($"\"worldName\":\"{JsonEscape(worldName)}\",");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"Could not retrieve all game info: {ex.Message}");
            }
            
            // Add SCON info
            sb.Append($"\"sconVersion\":\"{MyPluginInfo.PLUGIN_VERSION}\",");
            sb.Append($"\"sconHost\":\"{Plugin.ServerHost.Value}\",");
            sb.Append($"\"sconPort\":{Plugin.CurrentSconPort}");
            
            sb.Append("}");
            return sb.ToString();
        }
        
        private static string JsonEscape(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.Replace("\\", "\\\\")
                     .Replace("\"", "\\\"")
                     .Replace("\n", "\\n")
                     .Replace("\r", "\\r")
                     .Replace("\t", "\\t");
        }
    }
}
