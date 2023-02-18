using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "StatDatabase", menuName = "Game/" + nameof(OldStatDatabase), order = 1)]
public class OldStatDatabase : ScriptableObject
{
    public List<StatCollection> collections = new List<StatCollection>();

    public StatCollection GetCollection(string id) => collections.FirstOrDefault(c => c.id == id);

    public static OldStatDatabase Load()
    {
        OldStatDatabase db = null;
#if UNITY_EDITOR
        db = AssetDatabase.LoadAssetAtPath<OldStatDatabase>($"Assets/Resources/Databases/{nameof(OldStatDatabase)}.asset");
#endif
        return db;
    }
}