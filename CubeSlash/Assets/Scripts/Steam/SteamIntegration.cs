using Newtonsoft.Json;
using Steamworks;
using Steamworks.Data;
using System;
using System.Linq;
using System.Text;
using UnityEngine;

public class SteamIntegration : MonoBehaviour
{
    public static SteamIntegration Instance { get; private set; }

    private const uint STEAM_ID = 2814280;

    public static void Create()
    {
        if (Instance != null) return;

        var g = new GameObject(nameof(SteamIntegration));
        DontDestroyOnLoad(g);
        Instance = g.AddComponent<SteamIntegration>();
        Instance.InitializeClient();
    }

    private void InitializeClient()
    {
        try
        {
            SteamClient.Init(STEAM_ID);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void Update()
    {
        SteamClient.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        SteamClient.Shutdown();
    }

    public void SaveCloudData(string filename, string data)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            //Debug.Log($"{filename} size: {bytes.Length}");
            if (SteamClient.IsValid)
            {
                SteamRemoteStorage.FileWrite(filename, bytes);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public T LoadCloudData<T>(string filename)
    {
        try
        {
            var json = LoadCloudData(filename);
            T data = JsonConvert.DeserializeObject<T>(json);
            return data;
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to load {filename} from cloud data: {e.Message}");
        }

        return default;
    }

    private string LoadCloudData(string filename)
    {
        try
        {
            if (SteamClient.IsValid)
            {
                var bytes = SteamRemoteStorage.FileRead(filename);
                var data = Encoding.UTF8.GetString(bytes);
                return data;
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to save {filename} to cloud data: {e.Message}");
        }

        return null;
    }

    public void UnlockAchievement(AchievementType type) =>
        UnlockAchievement(type.ToString());

    public void UnlockAchievement(string id)
    {
        try
        {
            if (!SteamClient.IsValid) return;

            var achievements = SteamUserStats.Achievements.ToList();
            var has_achievement = achievements.Any(a => a.Identifier == id);
            var info = achievements.FirstOrDefault(a => a.Identifier == id);
            var is_unlocked = info.State;

            if (has_achievement && is_unlocked) return;

            var achievement = new Achievement(info.Identifier);
            achievement.Trigger();
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to unlock achievement {id}: {e.Message}");
        }
    }
}