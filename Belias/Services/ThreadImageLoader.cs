using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;

namespace Belias.Services;

/// <summary>
/// Thread-based image loading service inspired by ECommons ThreadLoadImageHandler
/// Loads images in a background thread to avoid UI hitching
/// </summary>
public static class ThreadImageLoader
{
    private class ImageLoadingResult
    {
        internal ISharedImmediateTexture? ImmediateTexture;
        internal IDalamudTextureWrap? TextureWrap;
        internal IDalamudTextureWrap? Texture => ImmediateTexture?.GetWrapOrDefault() ?? TextureWrap;
        internal bool IsCompleted = false;

        public ImageLoadingResult() { }

        public ImageLoadingResult(ISharedImmediateTexture? immediateTexture)
        {
            ImmediateTexture = immediateTexture;
        }

        public ImageLoadingResult(IDalamudTextureWrap? textureWrap)
        {
            TextureWrap = textureWrap;
        }
    }

    private static readonly ConcurrentDictionary<string, ImageLoadingResult> CachedTextures = new();
    private static readonly List<Func<byte[], byte[]>> ConversionsToBitmap = new() { b => b };
    private static volatile bool ThreadRunning = false;
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

    /// <summary>
    /// Attempts to load image from URL, game path or file on disk. Do NOT cache the textureWrap and call this function every time before you want to work with it.
    /// </summary>
    /// <param name="url">URL, game path or file on disk</param>
    /// <param name="textureWrap">Output texture wrap</param>
    /// <returns>True if texture was successfully loaded</returns>
    public static bool TryGetTextureWrap(string url, out IDalamudTextureWrap? textureWrap)
    {
        if (!CachedTextures.TryGetValue(url, out var result))
        {
            result = new ImageLoadingResult();
            CachedTextures[url] = result;
            BeginThreadIfNotRunning();
        }
        textureWrap = result.Texture;
        return result.Texture != null;
    }

    /// <summary>
    /// Clears and disposes all cached resources
    /// </summary>
    public static void ClearAll()
    {
        foreach (var x in CachedTextures)
        {
            try { x.Value.TextureWrap?.Dispose(); } catch (Exception ex) { Plugin.Log.Debug($"Error disposing texture: {ex.Message}"); }
        }
        CachedTextures.Clear();
    }    private static void BeginThreadIfNotRunning()
    {
        if (ThreadRunning) return;

        Plugin.Log.Debug("Starting ThreadImageLoader");
        ThreadRunning = true;

        new Thread(ImageLoaderThreadMethod).Start();
    }
    
    private static void ImageLoaderThreadMethod()
    {
        var idleTicks = 0;
        try
        {
            while (idleTicks < 100)
            {
                try
                {
                    if (CachedTextures.TryGetFirst(x => !x.Value.IsCompleted, out var keyValuePair))
                    {
                        idleTicks = 0;
                        keyValuePair.Value.IsCompleted = true;
                        Plugin.Log.Debug($"Loading image: {keyValuePair.Key}");

                        ProcessImageLoad(keyValuePair);
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error(ex, "Error in ThreadImageLoader processing loop");
                }

                idleTicks++;
                if (!CachedTextures.Any(x => !x.Value.IsCompleted)) Thread.Sleep(100);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "ThreadImageLoader thread crashed");
        }

        Plugin.Log.Debug($"Stopping ThreadImageLoader, ticks={idleTicks}");
        ThreadRunning = false;
    }
    
    private static void ProcessImageLoad(KeyValuePair<string, ImageLoadingResult> keyValuePair)
    {
        string key = keyValuePair.Key;
        
        if (key.StartsWith("http:", StringComparison.OrdinalIgnoreCase) || 
            key.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
        {
            LoadFromUrl(keyValuePair);
        }
        else if (key.StartsWith("embedded:"))
        {
            LoadEmbeddedImage(keyValuePair);
        }
        else if (File.Exists(key))
        {
            keyValuePair.Value.ImmediateTexture = Plugin.TextureProvider.GetFromFile(key);
        }
        else
        {
            // Try to load from game resources
            keyValuePair.Value.ImmediateTexture = Plugin.TextureProvider.GetFromGame(key);
        }
    }
    
    private static void LoadEmbeddedImage(KeyValuePair<string, ImageLoadingResult> keyValuePair)
    {
        // Handle embedded images using ImageLoaderService
        if (keyValuePair.Key == "embedded:belias-logo-base64")
        {
            keyValuePair.Value.TextureWrap = ImageLoaderService.LoadEmbeddedLogo();
        }
    }
    
    private static void LoadFromUrl(KeyValuePair<string, ImageLoadingResult> keyValuePair)
    {
        try
        {
            var result = HttpClient.GetAsync(keyValuePair.Key).Result;
            result.EnsureSuccessStatusCode();
            var content = result.Content.ReadAsByteArrayAsync().Result;
            
            IDalamudTextureWrap? texture = null;
            List<Exception> exceptions = new();

            foreach (var conversion in ConversionsToBitmap)
            {
                if (conversion == null) continue;

                try
                {
                    texture = Plugin.TextureProvider.CreateFromImageAsync(conversion(content)).Result;
                    if (texture != null) break;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0 && texture == null)
            {
                Plugin.Log.Error($"While loading {keyValuePair.Key}, exceptions occurred:");
                foreach (var ex in exceptions)
                {
                    Plugin.Log.Error(ex, "Exception details");
                }
            }

            keyValuePair.Value.TextureWrap = texture;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, $"Error downloading or processing image from URL: {keyValuePair.Key}");        }
    }/// <summary>
    /// Add a conversion function to transform image data before creating the texture
    /// </summary>
    public static void AddConversionToBitmap(Func<byte[], byte[]> conversion)
    {
        ConversionsToBitmap.Add(conversion);
    }
    
    /// <summary>
    /// Extension method to get first item matching a predicate from a ConcurrentDictionary
    /// </summary>
    private static bool TryGetFirst<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, 
                                                Func<KeyValuePair<TKey, TValue>, bool> predicate, 
                                                out KeyValuePair<TKey, TValue> result)
        where TKey : notnull
    {
        result = dictionary.FirstOrDefault(predicate);
        return !result.Equals(default(KeyValuePair<TKey, TValue>));
    }
}
