using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Game/Upgrade", order = 1)]
public class UpgradeData : ScriptableObject
{
    public enum Type
    {
        DASH_DISTANCE,
        DASH_TRAIL,
        SPLIT_RATE,
        SPLIT_ARC,
        CHARGE_AIM,
        CHARGE_WIDTH,
    }

    public Type type;
    public List<Level> levels = new List<Level>();

    [System.Serializable]
    public class Level
    {
        public string name;
        [TextArea] public string description;
        public Sprite icon;
    }

    private void OnValidate()
    {
        for (int i = 0; i < levels.Count; i++)
        {
            var level = levels[i];
            if (string.IsNullOrEmpty(level.name))
            {
                level.name = $"{type}_{i}";
            }
        }
    }
}