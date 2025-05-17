using System;
using System.IO;
using System.Collections.Generic;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin.Services;

namespace Belias.Services;

/// <summary>
/// Service that wraps icon retrieval.
/// </summary>
public static class IconService
{
    private static readonly Dictionary<uint, IDalamudTextureWrap> IconCache = new();
    private static readonly Dictionary<Job, IDalamudTextureWrap> JobIconMap = new();

    public static IDalamudTextureWrap? GetIcon(uint iconId)
    {
        if (Enum.IsDefined(typeof(Job), (int)iconId))
        {
            var job = (Job)iconId;
            if (JobIconMap.TryGetValue(job, out var jobTexture))
            {
                return jobTexture;
            }
        }

        if (IconCache.TryGetValue(iconId, out var texture))
        {
            return texture;
        }

        // Simulate loading an icon (replace with actual loading logic)
        texture = LoadIconFromSource(iconId);
        if (texture != null)
        {
            IconCache[iconId] = texture;
        }

        return texture;
    }

    public static void InitializeJobIcons()
    {
        // Populate JobIconMap with dummy data (replace with actual icon loading logic)
        JobIconMap[Job.WAR] = LoadIcon("WarriorIconPath");
        JobIconMap[Job.PLD] = LoadIcon("PaladinIconPath");
        // Add other jobs as needed
    }    private static IDalamudTextureWrap LoadIcon(string path)
    {
        // Simulate using the path parameter to load an icon
        Console.WriteLine($"Loading icon from path: {path}");
        // Return a placeholder texture object (replace with actual implementation)
        return new PlaceholderTextureWrap();
    }    /// <summary>
    /// Loads an image file from the specified path.
    /// </summary>
    /// <param name="path">Path to the image file</param>
    /// <returns>A texture wrap or null if loading failed</returns>
    public static IDalamudTextureWrap? LoadImageFromFile(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"Image file not found: {path}");
                return null;
            }
            
            Console.WriteLine($"Image file found at: {path}");
            
            // Try using the UiBuilder to load the image if available
            try
            {
                if (Plugin.PluginInterface != null)
                {                    // Use the TextureProvider instead of UiBuilder
                    try 
                    {
                        var texture = Plugin.TextureProvider.GetFromFile(path);
                        if (texture != null && texture.GetWrapOrDefault() != null)
                            return texture.GetWrapOrDefault();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading with TextureProvider: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error using UiBuilder to load image: {ex.Message}");
            }
            
            // Since we couldn't load the actual image, return a placeholder
            return new PlaceholderTextureWrap();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading image from {path}: {ex.Message}");
            return null;
        }
    }

    private static IDalamudTextureWrap? LoadIconFromSource(uint iconId)
    {
        // Simulate using iconId in the loading logic
        Console.WriteLine($"Loading icon with ID: {iconId}");
        return null; // Replace with actual texture loading
    }    /// <summary>
    /// A simple placeholder texture implementation when actual textures can't be loaded
    /// </summary>
    private sealed class PlaceholderTextureWrap : IDalamudTextureWrap
    {
        public IntPtr ImGuiHandle => IntPtr.Zero; // This signals UI to use fallback
        public int Width => 64;
        public int Height => 64;

        public void Dispose()
        {
            // No resources to dispose
        }
    }
}

public enum Job
{
    WAR,
    PLD,
    // Add other jobs as needed
}
