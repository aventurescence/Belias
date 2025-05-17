using System;
using System.IO;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin.Services;
using System.Numerics;

namespace Belias.Services;

/// <summary>
/// Service that handles image loading operations
/// </summary>
public static class ImageLoaderService
{
    private static readonly ConcurrentDictionary<string, IDalamudTextureWrap?> TextureCache = new();
    
    /// <summary>
    /// Loads an image from a file path
    /// </summary>
    /// <param name="path">Path to the image file (absolute)</param>
    /// <returns>A texture wrap or null if loading failed</returns>
    public static IDalamudTextureWrap? LoadImageFromFile(string path)
    {
        // First check if we already have this image cached
        if (TextureCache.TryGetValue(path, out var cachedTexture))
        {
            return cachedTexture;
        }
        
        // Validate inputs
        if (string.IsNullOrEmpty(path))
        {
            Plugin.Log.Error("ImageLoaderService: Cannot load from null or empty path");
            return null;
        }
        
        if (!File.Exists(path))
        {
            Plugin.Log.Error($"ImageLoaderService: File not found at path: {path}");
            return null;
        }
        
        try
        {
            // Method 1: Use ImageService which is known to work with the current API
            var texture = ImageService.LoadFromFile(path);
            if (texture != null)
            {
                Plugin.Log.Debug($"Successfully loaded image using ImageService: {path}");
                TextureCache[path] = texture;
                return texture;
            }
            
            // Method 2: Try loading from game resources in case it's actually a game path
            try
            {
                texture = ImageService.LoadFromGame(path);
                if (texture != null)
                {
                    Plugin.Log.Debug($"Successfully loaded image using LoadFromGame: {path}");
                    TextureCache[path] = texture;
                    return texture;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Debug($"Failed to load with LoadFromGame (expected for external files): {ex.Message}");
            }
            
            // If we reach here, all methods failed - log error but don't crash
            Plugin.Log.Warning($"All methods to load image failed for: {path}");
            return null;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, $"Error loading image from file: {path}");
            return null;
        }
    }
    
    /// <summary>
    /// Loads an image from a URL
    /// </summary>
    /// <param name="url">URL to the image file</param>
    /// <returns>A texture wrap or null if loading failed</returns>
    public static IDalamudTextureWrap? LoadImageFromUrl(string url)
    {
        // First check if we already have this image cached
        if (TextureCache.TryGetValue(url, out var cachedTexture))
        {
            return cachedTexture;
        }
        
        // Validate inputs
        if (string.IsNullOrEmpty(url))
        {
            Plugin.Log.Error("ImageLoaderService: Cannot load from null or empty URL");
            return null;
        }
          try
        {
            // GitHub raw content URL is different from GitHub web URL
            // Convert github.com URL to raw.githubusercontent.com if needed
            if (url.StartsWith("https://github.com/") && url.Contains("/blob/"))
            {
                url = url.Replace("github.com", "raw.githubusercontent.com")
                         .Replace("/blob/", "/");
                Plugin.Log.Information($"ImageLoaderService: Converted GitHub URL to raw content URL: {url}");
            }
            
            // Create a temporary file to store the downloaded image
            string tempFileName = Path.GetRandomFileName();
            string tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);
            
            Plugin.Log.Information($"ImageLoaderService: Attempting to download from URL: {url}");
            
            // Download the image using HttpClient (modern approach) with timeout
            using (var client = new System.Net.Http.HttpClient() { Timeout = TimeSpan.FromSeconds(10) })
            {
                // Add proper User-Agent to avoid being blocked
                client.DefaultRequestHeaders.Add("User-Agent", "Belias-Plugin/1.0");
                
                var response = client.GetAsync(url).Result;
                
                Plugin.Log.Information($"ImageLoaderService: HTTP response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    using (var fs = new FileStream(tempFilePath, FileMode.Create))
                    {
                        response.Content.CopyToAsync(fs).Wait();
                    }
                    
                    Plugin.Log.Information($"ImageLoaderService: Image saved to temp file: {tempFilePath}, Size: {new FileInfo(tempFilePath).Length} bytes");
                }
                else
                {
                    Plugin.Log.Error($"ImageLoaderService: Failed to download image, status code: {response.StatusCode}");
                    return null;
                }
            }
              // Verify the temp file was created and has content
            if (!File.Exists(tempFilePath) || new FileInfo(tempFilePath).Length == 0)
            {
                Plugin.Log.Error($"ImageLoaderService: Downloaded file either doesn't exist or is empty");
                return null;
            }
            
            // Try to load the image using our service
            Plugin.Log.Information($"ImageLoaderService: Loading image from temp file: {tempFilePath}");
            var texture = ImageService.LoadFromFile(tempFilePath);
            
            // If ImageService failed, try direct loading as fallback
            if (texture == null)
            {
                Plugin.Log.Information("ImageLoaderService: First load attempt failed, trying alternate method");
                try
                {
                    // Create a fallback texture using a different method if available
                    texture = Plugin.TextureProvider.GetFromFile(tempFilePath)?.GetWrapOrDefault();
                    Plugin.Log.Information($"ImageLoaderService: Fallback load result: {(texture != null ? "Success" : "Failed")}");
                }
                catch (Exception fallbackEx)
                {
                    Plugin.Log.Error(fallbackEx, "ImageLoaderService: Fallback loading method also failed");
                }
            }
            
            // Delete the temporary file
            try 
            { 
                File.Delete(tempFilePath); 
            } 
            catch (Exception ex) 
            {
                // Log but continue - not deleting the temp file isn't critical
                Plugin.Log.Debug($"Failed to delete temporary file {tempFilePath}: {ex.Message}");
            }
            
            // Cache and return the texture
            if (texture != null)
            {
                Plugin.Log.Information($"ImageLoaderService: Successfully loaded image from URL: {url} (Handle: {texture.ImGuiHandle})");
                TextureCache[url] = texture;
                return texture;
            }
            
            Plugin.Log.Error($"ImageLoaderService: Failed to load image from URL: {url}");
            return null;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, $"ImageLoaderService: Error loading image from URL: {url}");
            return null;
        }
    }
    
    /// <summary>
    /// Loads an image from an embedded base64 string to ensure we always have a logo
    /// </summary>
    /// <returns>A texture wrap or null if loading failed</returns>
    public static IDalamudTextureWrap? LoadEmbeddedLogo()
    {
        try
        {
            // Belias logo as embedded base64 data (small version)
            const string base64Logo = 
                "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAACXBIWXMAAAsTAAALEwEAmpwYAAAFEmlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS42LWMxNDIgNzkuMTYwOTI0LCAyMDE3LzA3LzEzLTAxOjA2OjM5ICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIgeG1sbnM6cGhvdG9zaG9wPSJodHRwOi8vbnMuYWRvYmUuY29tL3Bob3Rvc2hvcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RFdnQ9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZUV2ZW50IyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ0MgKFdpbmRvd3MpIiB4bXA6Q3JlYXRlRGF0ZT0iMjAyMy0wMS0xMlQxOTo0NzoxMSswMTowMCIgeG1wOk1vZGlmeURhdGU9IjIwMjMtMDEtMTJUMTk6NTI6NTErMDE6MDAiIHhtcDpNZXRhZGF0YURhdGU9IjIwMjMtMDEtMTJUMTk6NTI6NTErMDE6MDAiIGRjOmZvcm1hdD0iaW1hZ2UvcG5nIiBwaG90b3Nob3A6Q29sb3JNb2RlPSIzIiBwaG90b3Nob3A6SUNDUHJvZmlsZT0ic1JHQiBJRUM2MTk2Ni0yLjEiIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6ZDZjYzY5ZmQtMGMwMi0xYTQw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";

            // Create a temporary file
            string tempFileName = Path.GetRandomFileName();
            string tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

            try
            {
                // Convert base64 to binary and save to temp file
                byte[] imageData = Convert.FromBase64String(base64Logo);
                File.WriteAllBytes(tempFilePath, imageData);

                // Load the image from temp file
                var texture = ImageService.LoadFromFile(tempFilePath);
                
                // If that fails, try with TextureProvider directly
                if (texture == null)
                {
                    texture = Plugin.TextureProvider.GetFromFile(tempFilePath)?.GetWrapOrDefault();
                }                // Cleanup temp file
                try 
                { 
                    File.Delete(tempFilePath); 
                } 
                catch (Exception ex) 
                {
                    // Non-critical error, just log it
                    Plugin.Log.Debug($"Failed to delete temporary file: {ex.Message}");
                }

                if (texture != null)
                {
                    Plugin.Log.Information("Successfully loaded embedded logo image");
                    return texture;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Error(ex, "Error loading embedded logo");
            }
            finally
            {
                try 
                { 
                    if (File.Exists(tempFilePath)) 
                        File.Delete(tempFilePath); 
                } 
                catch (Exception ex) 
                {
                    // Cleanup failure is not critical
                    Plugin.Log.Debug($"Failed to clean up temp file in finally block: {ex.Message}");
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "Error in LoadEmbeddedLogo");
            return null;
        }
    }
    
    /// <summary>
    /// Clears all cached textures
    /// </summary>
    public static void ClearCache()
    {
        foreach (var texture in TextureCache.Values)
        {
            texture?.Dispose();
        }
        
        TextureCache.Clear();
    }
}
