using UnityEngine;

public static class Level
{
    public static LevelAsset Current { get { return LevelDatabase.Instance.GetLevel(GameController.Instance.LevelIndex); } }

    public static void Completed()
    {
        GameController.Instance.LevelIndex++;
    }
}