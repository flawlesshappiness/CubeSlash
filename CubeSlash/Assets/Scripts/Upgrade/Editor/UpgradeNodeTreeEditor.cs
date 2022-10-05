using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UpgradeNodeTree))]
public class UpgradeNodeTreeEditor : Editor
{
    private UpgradeNodeTree tree;
    private UpgradeDatabase database;

    private void OnEnable()
    {
        tree = target as UpgradeNodeTree;
        database = AssetDatabase.LoadAssetAtPath<UpgradeDatabase>($"Assets/Resources/Databases/{nameof(UpgradeDatabase)}.asset");
    }

    public override void OnInspectorGUI()
    {
        GUIHelper.DrawAssetSaveButton(tree);

        // Required ability
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        GUIHelper.FittedLabel("Require ability");
        tree.require_ability = GUILayout.Toggle(tree.require_ability, "", GUILayout.Width(20));

        if (tree.require_ability)
        {
            tree.ability_type_required = (Ability.Type)EditorGUILayout.EnumPopup(tree.ability_type_required);
        }
        else
        {
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(tree); }

        GUILayout.Space(20);

        if(GUILayout.Button($"Open Tree ({tree.nodes.Count} nodes)", GUILayout.Height(40)))
        {
            UpgradeNodeTreeWindow.Open(tree);
        }

        if (database.trees.Contains(tree))
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUIHelper.PushColor(Color.Lerp(Color.green, Color.white, 0.4f));
            GUIHelper.CenterLabel("Exists in database", GUILayout.Height(30));
            GUIHelper.PopColor();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Remove from database", GUILayout.Width(200), GUILayout.Height(30)))
            {
                database.trees.Remove(tree);
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add to database", GUILayout.Width(200), GUILayout.Height(30)))
            {
                database.trees.Add(tree);
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
            }
            GUILayout.FlexibleSpace();

            GUIHelper.PushColor(Color.Lerp(Color.red, Color.white, 0.4f));
            GUIHelper.CenterLabel("Not in database", GUILayout.Height(30));
            GUIHelper.PopColor();
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }
    }
}