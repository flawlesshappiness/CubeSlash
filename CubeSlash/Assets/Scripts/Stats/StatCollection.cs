using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "StatCollection", menuName = "Game/StatCollection", order = 1)]
public class StatCollection : ScriptableObject
{
    public string id;
    public List<StatParameter> stats = new List<StatParameter>();

    public StatParameter GetStat(string id) => stats.FirstOrDefault(stat => stat.name == id);

    public static StatCollection Load(string id)
    {
#if UNITY_EDITOR
        var db = OldStatDatabase.Load();
        return db.collections.FirstOrDefault(c => c.id == id);
#endif
        return null;
    }
}