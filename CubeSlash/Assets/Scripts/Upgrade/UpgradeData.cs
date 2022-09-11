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

        PLAYER_EXP,
        PLAYER_HEALTH,
        PLAYER_SPEED,
    }

    public Type type;
    public bool require_ability;
    public Ability.Type type_ability_required;
    public UpgradeNodeTree tree;
    public List<Level> levels = new List<Level>();

    [System.Serializable]
    public class Level
    {
        public string name;
        public Sprite icon;
        public List<string> desc_positive;
        public List<string> desc_negative;
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