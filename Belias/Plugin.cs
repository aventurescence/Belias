using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Belias.Windows;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Textures.TextureWraps;
using System;
using System.IO;
using Belias.Services;

namespace Belias;

public sealed class Plugin : IDalamudPlugin
{
    public static string Name => "Belias"; // Made static to match usages
    
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    
    // Plugin logo locations for ThreadImageLoader
    internal static readonly string[] LogoLocations = new string[3];
    
    private const string CommandName = "/belias";
    
    public Configuration Configuration { get; init; }    
    public readonly WindowSystem WindowSystem = new("Belias");
    private MainWindow MainWindow { get; init; }
    private ConfigWindow ConfigWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
          // Set up logo loading urls
        LogoLocations[0] = "https://raw.githubusercontent.com/aventurescence/Belias/master/Belias/Assets/Belias.png";
        LogoLocations[1] = Path.Combine(PluginInterface.AssemblyLocation.Directory!.FullName, "Assets", "Belias.png");
        LogoLocations[2] = "embedded:belias-logo-base64"; // Special identifier for embedded logo
        
        Log.Information($"Setting up logo for thread-based loading");
        
        // Pre-register the logos with ThreadImageLoader for background loading
        if (ThreadImageLoader.TryGetTextureWrap(LogoLocations[0], out var _))
        {
            Log.Debug("GitHub logo URL was registered for thread loading");
        }
        
        if (File.Exists(LogoLocations[1]) && ThreadImageLoader.TryGetTextureWrap(LogoLocations[1], out var _))
        {
            Log.Debug("Local logo path was registered for thread loading");
        }
          // Initialize windows
        MainWindow = new MainWindow();
        ConfigWindow = new ConfigWindow(this);
        
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open Belias main window"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        Log.Information($"Belias plugin loaded");
    }
    
    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        
        // Dispose windows
        MainWindow?.Dispose();
        
        // Remove command handler
        CommandManager.RemoveHandler(CommandName);
        
        // Clean up all image caches
        ThreadImageLoader.ClearAll();
        ImageLoaderService.ClearCache();
    }    private void OnCommand(string command, string args)
    {
        ToggleMainUI();
    }

    private void ToggleMainUI()
    {
        MainWindow.IsOpen = !MainWindow.IsOpen;
    }

    private void DrawUI()
    {
        WindowSystem.Draw();
    }
    
    // Additional toggles for external access
    public void ToggleMainWindow() => MainWindow.Toggle();
    public void ToggleConfigWindow() => ConfigWindow.Toggle();
}
