using UnityEngine;

public static class Level
{
    public static LevelAsset Current { get { return LevelDatabase.Instance.levels[Mathf.Clamp(Data.Game.idx_level, 0, LevelDatabase.Instance.levels.Count - 1)]; } }

    public static void Completed()
    {
        Data.Game.idx_level++;
        Data.SaveGameData();
    }
}