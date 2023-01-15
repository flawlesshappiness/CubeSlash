using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(LevelDatabase), menuName = "Level/LevelDatabase", order = 1)]
public class LevelDatabase : ScriptableObject
{
    private static LevelDatabase _instance;
    public static LevelDatabase Instance { get { return _instance ?? Load(); } }

    public List<LevelAsset> levels = new List<LevelAsset>();

    private static LevelDatabase Load()
    {
        if(_instance == null)
        {
            _instance = Resources.Load<LevelDatabase>("Databases/" + nameof(LevelDatabase));
        }
        return _instance;
    }
}
