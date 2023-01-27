using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(AreaDatabase), menuName = "Game/" + nameof(AreaDatabase), order = 1)]
public class AreaDatabase : ScriptableObject
{
    public List<Area> areas = new List<Area>();

    public static AreaDatabase Instance { get { return _instance ?? LoadAsset(); } }
    private static AreaDatabase _instance;

    public static AreaDatabase LoadAsset()
    {
        if(_instance == null)
        {
            _instance = Resources.Load<AreaDatabase>("Databases/AreaDatabase");
        }
        return _instance;
    }
}