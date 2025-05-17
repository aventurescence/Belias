using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace Belias;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    // UI Settings
    public bool DarkMode { get; set; } = true;
    public bool ShowWelcomeOnStartup { get; set; } = true;
    public string AccentColor { get; set; } = "#C83C23";
    
    // Application Settings
    public bool AutoStart { get; set; } = false;
    public int RefreshRate { get; set; } = 60;
    
    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
