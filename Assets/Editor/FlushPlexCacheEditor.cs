using UnityEngine;
using UnityEditor;
using System.IO;

public static class FlushPlexCacheEditor
{
    [MenuItem("WatchOrNot/Flush Cache/This Playlist")]
    public static void FlushSingleCache()
    {
        string playlistName = EditorUtility.DisplayDialogComplex("Flush Cache", "Which playlist cache do you want to delete?", "Watch or Not", "Cancel", "Custom") switch
        {
            0 => "Watch or Not",
            2 => EditorUtility.DisplayDialog("Enter Playlist", "This feature is not implemented yet. Use 'All' for now.", "OK") ? "CustomPlaylist" : null,
            _ => null
        };

        if (playlistName == null)
            return;

        DeleteCacheForPlaylist(playlistName);
    }

    [MenuItem("WatchOrNot/Flush Cache/All Playlists")]
    public static void FlushAllCaches()
    {
        string path = Application.persistentDataPath;
        string[] files = Directory.GetFiles(path, "watch_or_not_*_cache.json");

        foreach (var file in files)
        {
            File.Delete(file);
            Debug.Log($"Deleted: {file}");
        }

        string posterCachePath = Path.Combine(path, "PosterCache");
        if (Directory.Exists(posterCachePath))
        {
            Directory.Delete(posterCachePath, true);
            Debug.Log("Deleted all poster cache.");
        }

    }
    private static void DeleteCacheForPlaylist(string playlistName)
    {
        string cacheFile = Path.Combine(Application.persistentDataPath, $"watch_or_not_{playlistName}_cache.json");
        string posterCache = Path.Combine(Application.persistentDataPath, "PosterCache");

        if (File.Exists(cacheFile))
        {
            File.Delete(cacheFile);
            Debug.Log($"Deleted cache file for playlist '{playlistName}'.");
        }
        else
        {
            Debug.LogWarning($"No cache file found for playlist '{playlistName}'.");
        }

        if (Directory.Exists(posterCache))
        {
            Directory.Delete(posterCache, true);
            Debug.Log("Deleted poster cache.");
        }

        AssetDatabase.Refresh();
    }
}
