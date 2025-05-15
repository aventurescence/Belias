using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace Belias.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private bool disposedValue;    public MainWindow(Plugin plugin)
        : base("Belias - Rotation Editor##MainWindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // Dispose managed resources here if needed.
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public override void Draw()
    {
        DrawHeader();
        DrawConfigurationSection();
        DrawRotationSection();
        DrawPlayerInformationSection();
        DrawFooter();
    }    private void DrawHeader()
    {
        // Use Belias.png as the logo from the Assets folder in output directory
        try
        {
            var assemblyDir = Plugin.PluginInterface.AssemblyLocation.Directory?.FullName!;
            var logoPath = System.IO.Path.Combine(assemblyDir, "Assets", "Belias.png");
            
            if (!System.IO.File.Exists(logoPath))
            {
                // Try looking in another location
                logoPath = System.IO.Path.Combine(assemblyDir, "Belias.png");
            }
            
            var logoImage = Plugin.TextureProvider.GetFromFile(logoPath).GetWrapOrDefault();
            if (logoImage != null)
            {
                float scale = 0.8f; // Scale down the logo if needed
                ImGui.Image(logoImage.ImGuiHandle, new Vector2(logoImage.Width * scale, logoImage.Height * scale));
            }
        }
        catch (Exception ex)
        {
            // If we can't load the image, just don't display it
            Plugin.Log.Error($"Failed to load logo: {ex.Message}");
        }
        
        ImGui.TextUnformatted("Belias - Rotation Editor");
        ImGui.Separator();
        ImGui.TextUnformatted("Create and manage rotations for Rotation Solver Reborn with ease.");
        ImGui.Spacing();
        
        // Reference class member to ensure non-static
        _ = plugin;
    }

    private void DrawConfigurationSection()
    {
        ImGui.TextUnformatted("Configuration:");
        ImGui.Indent();
        ImGui.TextUnformatted($"Random config bool: {plugin.Configuration.SomePropertyToBeSavedAndWithADefault}");
        if (ImGui.Button("Show Settings"))
        {
            plugin.ToggleConfigUI();
        }
        ImGui.Unindent();
        ImGui.Spacing();
    }

    private void DrawRotationSection()
    {
        ImGui.TextUnformatted("Rotation Management:");
        ImGui.Indent();
          if (ImGui.Button("Open Node Editor"))
        {
            // Open the node editor window
            plugin.ToggleNodeEditorUI();
        }
        
        ImGui.SameLine();
        
        if (ImGui.Button("Create New Rotation"))
        {
            // Implementation for creating a new rotation
        }
        
        ImGui.Unindent();
        ImGui.Spacing();
        
        // Reference class member to ensure non-static
        _ = plugin;
    }

    private void DrawPlayerInformationSection()
    {
        ImGui.TextUnformatted("Player Information:");
        ImGui.Indent();
        var localPlayer = Plugin.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            ImGui.TextUnformatted("Local player is not loaded.");
        }
        else if (!localPlayer.ClassJob.IsValid)
        {
            ImGui.TextUnformatted("Current job is not valid.");
        }
        else
        {
            ImGui.TextUnformatted($"Current job: ({localPlayer.ClassJob.RowId}) \"{localPlayer.ClassJob.Value.Abbreviation.ExtractText()}\"");
            var territoryId = Plugin.ClientState.TerritoryType;
            if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
            {
                ImGui.TextUnformatted($"Current area: ({territoryId}) \"{territoryRow.PlaceName.Value.Name.ExtractText()}\"");
            }
            else
            {
                ImGui.TextUnformatted("Invalid territory.");
            }
        }
        ImGui.Unindent();
        ImGui.Spacing();
        
        // Reference class member to ensure non-static
        _ = plugin;
    }

    private void DrawFooter()
    {
        ImGui.Separator();
        ImGui.TextUnformatted("Thank you for using Belias Plugin!");
        
        // Reference class member to ensure non-static
        _ = plugin;
    }
}
