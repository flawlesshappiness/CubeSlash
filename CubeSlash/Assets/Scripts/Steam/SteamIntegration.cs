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

    private const bool DISABLED = false;

    public static void Create()
    {
        if (Instance != null) return;

        LogController.LogMethod();

        var g = new GameObject(nameof(SteamIntegration));
        DontDestroyOnLoad(g);
        Instance = g.AddComponent<SteamIntegration>();
        Instance.InitializeClient();
    }

    private void InitializeClient()
    {
        try
        {
            if (DISABLED)
            {
                LogController.LogMessage($"STEAM: SteamClient not initialized in UNITY_EDITOR");
            }
            else
            {
                LogController.LogMessage($"STEAM: Initializing SteamClient");
                SteamClient.Init(STEAM_ID);
            }
        }
        catch (Exception e)
        {
            LogController.LogException(e);
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
            LogController.LogMessage($"STEAM: Saving cloud data {filename}");

            var bytes = Encoding.UTF8.GetBytes(data);
            if (SteamClient.IsValid)
            {
                SteamRemoteStorage.FileWrite(filename, bytes);
            }
        }
        catch (Exception e)
        {
            LogController.LogException(e);
        }
    }

    public T LoadCloudData<T>(string filename)
    {
        try
        {
            LogController.LogMessage($"STEAM: Loading cloud data {filename}");

            var json = LoadCloudData(filename);
            T data = JsonUtility.FromJson<T>(json);
            return data;
        }
        catch (Exception e)
        {
            LogController.LogMessage($"Failed to deserialize {filename} from cloud data");
            LogController.LogException(e);
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
            LogController.LogMessage($"Failed to save {filename} to cloud data");
            LogController.LogException(e);
        }

        return null;
    }

    public void UnlockAchievement(AchievementType type) =>
        UnlockAchievement(type.ToString());

    public void UnlockAchievement(string id)
    {
        try
        {
            LogController.LogMessage($"STEAM: Unlock achievement: {id}");
            Debug.Log($"STEAM: Unlock achievement: {id}");

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
            LogController.LogMessage($"Failed to unlock achievement {id}");
            LogController.LogException(e);
        }
    }
}