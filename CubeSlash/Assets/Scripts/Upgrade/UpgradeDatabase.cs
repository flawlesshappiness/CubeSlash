using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/UpgradeDatabase", fileName = nameof(UpgradeDatabase), order = 0)]
public class UpgradeDatabase : ScriptableObject
{
    public List<Upgrade> upgrades = new List<Upgrade>();
    public List<UpgradeNodeTree> trees = new List<UpgradeNodeTree>();

    public static UpgradeDatabase LoadAsset()
    {
        UpgradeDatabase db = null;
#if UNITY_EDITOR
        db = AssetDatabase.LoadAssetAtPath<UpgradeDatabase>($"Assets/Resources/Databases/{nameof(UpgradeDatabase)}.asset");
#endif
        return db;
    }

    public static UpgradeDatabase LoadResource()
    {
        return Resources.Load<UpgradeDatabase>($"Databases/{nameof(UpgradeDatabase)}");
    }
}