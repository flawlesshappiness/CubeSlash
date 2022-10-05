using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "StatDatabase", menuName = "Game/StatDatabase", order = 1)]
public class StatDatabase : ScriptableObject
{
    public List<StatCollection> collections = new List<StatCollection>();

    public StatCollection GetCollection(string id) => collections.FirstOrDefault(c => c.id == id);

    public static StatDatabase Load()
    {
        StatDatabase db = null;
#if UNITY_EDITOR
        db = AssetDatabase.LoadAssetAtPath<StatDatabase>($"Assets/Resources/Databases/{nameof(StatDatabase)}.asset");
#endif
        return db;
    }
}