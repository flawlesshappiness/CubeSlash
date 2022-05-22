using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(LevelDatabase), menuName = "Level/LevelDatabase", order = 1)]
public class LevelDatabase : ScriptableObject
{
    private static LevelDatabase _instance;
    public static LevelDatabase Instance { get { return _instance ?? Resources.Load<LevelDatabase>("Databases/" + nameof(LevelDatabase)); } }

    public List<LevelAsset> levels = new List<LevelAsset>();
}
