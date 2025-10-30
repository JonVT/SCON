using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace SCON
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }

        // Configuration
        public static ConfigEntry<bool> ServerEnabled;
        public static ConfigEntry<string> ServerHost;
        public static ConfigEntry<int> ServerPort;
        public static ConfigEntry<string> ServerApiKey;
        public static ConfigEntry<bool> AutoBindToServerPortPlusOne;

    // Actual bound RCON port (may differ from configured ServerPort when auto-binding)
    public static int CurrentRconPort;

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
            ServerApiKey = Config.Bind("Server",
                "ApiKey",
                "",
                "API key for authentication. Leave empty to allow localhost without key. Network connections always require a key.");

            AutoBindToServerPortPlusOne = Config.Bind("Server",
                "AutoBindToServerPortPlusOne",
                true,
                "If true and running a dedicated server, bind RCON to (game server port + 1). Otherwise use configured Port.");

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} is loaded!");

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

                // Decide which port to bind
                int desiredPort = ServerPort.Value;
                bool isDedicated = false;
                int detectedServerPort;
                if (AutoBindToServerPortPlusOne.Value && GameInfoCollector.TryGetServerPort(out detectedServerPort, out isDedicated) && isDedicated)
                {
                    desiredPort = detectedServerPort + 1;
                    Logger.LogInfo($"Auto-binding RCON to dedicated server port+1: {desiredPort} (server port {detectedServerPort})");
                }

                CurrentRconPort = desiredPort;

                httpServer = new HttpServerManager();
                httpServer.Initialize(ServerHost.Value, desiredPort);
                Logger.LogInfo($"RCON server started on {ServerHost.Value}:{desiredPort}");
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
