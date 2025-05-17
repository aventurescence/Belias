using System;
using System.IO;
using System.Collections.Concurrent;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin.Services;

namespace Belias.Services;

/// <summary>
/// Service that handles image loading operations using Dalamud TextureProvider
/// </summary>
public static class ImageService
{
    private static readonly ConcurrentDictionary<string, IDalamudTextureWrap?> FileTextureCache = new();
    private static readonly ConcurrentDictionary<uint, IDalamudTextureWrap?> IconCache = new();
    
    /// <summary>
    /// Loads an image from a file path
    /// </summary>
    /// <param name="path">Path to the image file (absolute)</param>
    /// <returns>A texture wrap or null if loading failed</returns>
    public static IDalamudTextureWrap? LoadFromFile(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Plugin.Log.Error("ImageService: Cannot load from null or empty path");
            return null;
        }
        
        if (FileTextureCache.TryGetValue(path, out var cachedTexture))
        {
            return cachedTexture;
        }
        
        try
        {
            if (!File.Exists(path))
            {                Plugin.Log.Error($"ImageService: File not found at path: {path}");
                return null;
            }
              // Load the texture using TextureProvider
            var texture = Plugin.TextureProvider.GetFromFile(path);
            if (texture != null && texture.GetWrapOrDefault() != null)
            {
                // Get the wrap that can be used with ImGui
                var wrap = texture.GetWrapOrDefault();
                // Cache the texture
                FileTextureCache[path] = wrap;
                return wrap;
            }
            
            Plugin.Log.Error($"ImageService: Failed to load texture from file: {path}");
            return null;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, $"ImageService: Error loading image from file: {path}");
            return null;
        }
    }
    
    /// <summary>
    /// Loads an image from a game icon ID
    /// </summary>
    /// <param name="iconId">Game icon ID</param>
    /// <returns>A texture wrap or null if loading failed</returns>
    public static IDalamudTextureWrap? LoadGameIcon(uint iconId)
    {
        if (IconCache.TryGetValue(iconId, out var cachedTexture))
        {
            return cachedTexture;
        }
        
        try
        {            // Load the icon using TextureProvider
            var texture = Plugin.TextureProvider.GetFromGameIcon(iconId);
            if (texture != null && texture.GetWrapOrDefault() != null)
            {
                // Cache the texture
                IconCache[iconId] = texture.GetWrapOrDefault();
                return texture.GetWrapOrDefault();
            }
            
            Plugin.Log.Error($"ImageService: Failed to load game icon: {iconId}");
            return null;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, $"ImageService: Error loading game icon: {iconId}");
            return null;
        }
    }
    
    /// <summary>
    /// Loads an image from a game file path
    /// </summary>
    /// <param name="path">Game file path</param>
    /// <returns>A texture wrap or null if loading failed</returns>
    public static IDalamudTextureWrap? LoadFromGame(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Plugin.Log.Error("ImageService: Cannot load from null or empty path");
            return null;
        }
        
        if (FileTextureCache.TryGetValue(path, out var cachedTexture))
        {
            return cachedTexture;
        }
        
        try
        {
            // Load the texture using TextureProvider
            var texture = Plugin.TextureProvider.GetFromGame(path);
            if (texture != null && texture.GetWrapOrDefault() != null)
            {
                // Cache the texture
                FileTextureCache[path] = texture.GetWrapOrDefault();
                return texture.GetWrapOrDefault();
            }
            
            Plugin.Log.Error($"ImageService: Failed to load texture from game path: {path}");
            return null;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, $"ImageService: Error loading image from game path: {path}");
            return null;
        }
    }
    
    /// <summary>
    /// Clears all cached textures
    /// </summary>
    public static void ClearCache()
    {
        foreach (var texture in FileTextureCache.Values)
        {
            texture?.Dispose();
        }
        
        foreach (var texture in IconCache.Values)
        {
            texture?.Dispose();
        }
        
        FileTextureCache.Clear();
        IconCache.Clear();
    }
}
