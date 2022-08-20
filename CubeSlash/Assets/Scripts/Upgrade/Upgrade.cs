using UnityEngine;

public class Upgrade
{
    public UpgradeData data;
    public int level;

    public int LevelsRemain { get { return (data.levels.Count + 1) - level; } }
    public bool IsMaxLevel { get { return LevelsRemain <= 1; } }

    public UpgradeData.Level GetCurrentLevel() => data.levels[level];
}