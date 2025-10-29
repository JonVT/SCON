using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace StationeersRCON
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }

        // Configuration
        public static ConfigEntry<bool> ServerEnabled;
        public static ConfigEntry<string> ServerHost;
        public static ConfigEntry<int> ServerPort;

        private HttpServerManager httpServer;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            // Prevent this GameObject from being destroyed on scene changes
            DontDestroyOnLoad(gameObject);

            // Setup configuration
            ServerEnabled = Config.Bind("Server",
                "Enabled",
                true,
                "Enable or disable the RCON server");

            ServerHost = Config.Bind("Server",
                "Host",
                "localhost",
                "Host address to bind the server to (use * for all interfaces)");

            ServerPort = Config.Bind("Server",
                "Port",
                8080,
                "Port number for the RCON server");

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");

            if (ServerEnabled.Value)
            {
                StartServer();
            }
        }

        private void StartServer()
        {
            try
            {
                // Ensure main thread dispatcher exists
                var dispatcher = UnityMainThreadDispatcher.Instance;
                
                httpServer = new HttpServerManager();
                httpServer.Initialize(ServerHost.Value, ServerPort.Value);
                Logger.LogInfo($"RCON server started on {ServerHost.Value}:{ServerPort.Value}");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Failed to start RCON server: {ex.Message}");
            }
        }

        private void Update()
        {
            // Update method no longer needed - UnityMainThreadDispatcher handles it
        }

        private void OnDestroy()
        {
            // Don't stop the server - let it keep running
        }

        private void OnApplicationQuit()
        {
            Logger.LogInfo("Application quitting - stopping RCON server");
            if (httpServer != null)
            {
                httpServer.Stop();
            }
        }

        private void OnDisable()
        {
            // Don't stop the server - let it keep running
        }
    }
}
