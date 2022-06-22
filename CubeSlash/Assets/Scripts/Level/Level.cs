using UnityEngine;

public static class Level
{
    public static LevelAsset Current { get { return LevelDatabase.Instance.levels[Mathf.Clamp(GameController.Instance.LevelIndex, 0, LevelDatabase.Instance.levels.Count - 1)]; } }

    public static void Completed()
    {
        GameController.Instance.LevelIndex++;
    }
}