using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AbilityModifierCollection))]
public class AbilityModifierCollectionEditor : Editor
{
    private AbilityModifierCollection collection;
    private void OnEnable()
    {
        collection = target as AbilityModifierCollection;
    }

    public override void OnInspectorGUI()
    {
        GUIHelper.DrawAssetSaveButton(collection);

        EditorGUI.BeginChangeCheck();
        foreach(var modifier in collection.modifiers)
        {
            DrawModifier(modifier);
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(collection);
        }
    }

    private void DrawModifier(AbilityModifier modifier)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(modifier.type.ToString(), GUILayout.Width(150));
        modifier.upgrade = EditorGUILayout.ObjectField(modifier.upgrade, typeof(Upgrade), false) as Upgrade;
        GUILayout.EndHorizontal();
        modifier.description = EditorGUILayout.TextArea(modifier.description, GUILayout.Height(60));
    }
}