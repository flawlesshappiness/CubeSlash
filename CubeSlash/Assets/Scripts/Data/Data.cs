using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Data
{
    public static GameSaveData Game { get { return SaveDataController.Instance.Get<GameSaveData>(); } }

    public static void SaveGameData()
    {
        SaveDataController.Instance.Save<GameSaveData>();
    }
}
