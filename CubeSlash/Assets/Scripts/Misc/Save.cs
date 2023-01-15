using UnityEngine;

public static class Save
{
    public static GameSaveData Game { get { return SaveDataController.Instance.Get<GameSaveData>(); } }
}