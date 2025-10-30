namespace SCON
{
    // Back-compat shim: delegate to auto-generated MyPluginInfo (from BepInEx.PluginInfoProps)
    public static class PluginInfo
    {
        public const string PLUGIN_GUID = MyPluginInfo.PLUGIN_GUID;
        public const string PLUGIN_NAME = MyPluginInfo.PLUGIN_NAME;
        public const string PLUGIN_VERSION = MyPluginInfo.PLUGIN_VERSION;
    }
}
