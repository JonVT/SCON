using System;
using System.Reflection;
using System.Text;

namespace StationeersRCON
{
    public static class GameInfoCollector
    {
        public static string GetGameInfo()
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"success\":true,");
            
            try
            {
                // Try to get NetworkManager info
                var assemblyCSharp = Assembly.Load("Assembly-CSharp");
                var networkManagerType = assemblyCSharp.GetType("Assets.Scripts.Networking.NetworkManager")
                    ?? assemblyCSharp.GetType("NetworkManager");
                
                if (networkManagerType != null)
                {
                    var instanceProp = networkManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    var instanceField = networkManagerType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
                    
                    object networkManager = instanceProp?.GetValue(null) ?? instanceField?.GetValue(null);
                    
                    if (networkManager != null)
                    {
                        // Try to get server port
                        var portProp = networkManagerType.GetProperty("ServerPort", BindingFlags.Public | BindingFlags.Instance);
                        var portField = networkManagerType.GetField("ServerPort", BindingFlags.Public | BindingFlags.Instance)
                            ?? networkManagerType.GetField("serverPort", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            ?? networkManagerType.GetField("_serverPort", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        
                        int? port = null;
                        if (portProp != null)
                        {
                            port = (int?)portProp.GetValue(networkManager);
                        }
                        else if (portField != null)
                        {
                            port = (int?)portField.GetValue(networkManager);
                        }
                        
                        if (port.HasValue)
                        {
                            sb.Append($"\"serverPort\":{port.Value},");
                        }
                        
                        // Try to get if server is running
                        var isServerProp = networkManagerType.GetProperty("IsServer", BindingFlags.Public | BindingFlags.Instance);
                        var isServerField = networkManagerType.GetField("IsServer", BindingFlags.Public | BindingFlags.Instance)
                            ?? networkManagerType.GetField("isServer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        
                        bool? isServer = null;
                        if (isServerProp != null)
                        {
                            isServer = (bool?)isServerProp.GetValue(networkManager);
                        }
                        else if (isServerField != null)
                        {
                            isServer = (bool?)isServerField.GetValue(networkManager);
                        }
                        
                        if (isServer.HasValue)
                        {
                            sb.Append($"\"isServer\":{isServer.Value.ToString().ToLower()},");
                        }
                    }
                }
                
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
                        var worldNameProp = worldManagerType.GetProperty("WorldName", BindingFlags.Public | BindingFlags.Instance);
                        var worldNameField = worldManagerType.GetField("WorldName", BindingFlags.Public | BindingFlags.Instance)
                            ?? worldManagerType.GetField("worldName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        
                        string worldName = null;
                        if (worldNameProp != null)
                        {
                            worldName = worldNameProp.GetValue(worldManager) as string;
                        }
                        else if (worldNameField != null)
                        {
                            worldName = worldNameField.GetValue(worldManager) as string;
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
            
            // Add RCON info
            sb.Append($"\"rconVersion\":\"1.0.0\",");
            sb.Append($"\"rconHost\":\"{Plugin.ServerHost.Value}\",");
            sb.Append($"\"rconPort\":{Plugin.ServerPort.Value}");
            
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
