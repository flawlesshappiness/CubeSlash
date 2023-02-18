using UnityEditor;
using UnityEngine;

public static class CreateStats
{
    [MenuItem("Game/Stats/Create all stats")]
    private static void CreateAllStats()
    {
        ExtraEditorUtility.EnsureDirectoryExists(EditorPaths.STATS);

        var types = FakeEnum.GetAll(typeof(StatID));
        foreach(var type in types)
        {
            var name = type.ToString();
            var dir = $"{EditorPaths.STATS}/{name.Split('_')[0]}";
            var path = $"{dir}/{name}.asset";
            var stat = ScriptableObject.CreateInstance<Stat>();
            stat.id = (StatID)type;

            ExtraEditorUtility.EnsureDirectoryExists(dir);
            AssetDatabase.CreateAsset(stat, path);
        }

        AssetDatabase.SaveAssets();
    }
}