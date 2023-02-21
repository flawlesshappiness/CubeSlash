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
        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(collection);
        }
    }
}