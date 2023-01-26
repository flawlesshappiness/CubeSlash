using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(LevelDatabase), menuName = "Level/LevelDatabase", order = 1)]
public class LevelDatabase : ScriptableObject
{
    private static LevelDatabase _instance;
    public static LevelDatabase Instance { get { return _instance ?? LoadAsset(); } }

    [SerializeField] private List<LevelAsset> levels = new List<LevelAsset>();

    [Header("TESTING")]
    public LevelAsset test_level;
    public bool testing_enabled;

    public static LevelDatabase LoadAsset()
    {
        if(_instance == null)
        {
            _instance = Resources.Load<LevelDatabase>("Databases/" + nameof(LevelDatabase));
        }
        return _instance;
    }

    public LevelAsset GetLevel(int i)
    {
        return testing_enabled ? test_level : levels[Mathf.Clamp(i, 0, levels.Count - 1)];
    }

    public int GetLevelCount() => levels.Count;
}
