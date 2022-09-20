using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Ability), true)]
public class AbilityEditor : Editor
{
    private Ability ability;

    private bool foldBase;

    private void OnEnable()
    {
        ability = target as Ability;
    }

    public override void OnInspectorGUI()
    {
        // Icon
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        ability.sprite_icon = (Sprite)EditorGUILayout.ObjectField(ability.sprite_icon, typeof(Sprite), false, GUILayout.Height(60), GUILayout.Width(60));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        // Name
        GUILayout.BeginHorizontal();
        GUILayout.Label("Name", GUILayout.Width(100));
        ability.name = GUILayout.TextField(ability.name);
        GUILayout.EndHorizontal();

        // Description
        GUILayout.BeginHorizontal();
        GUILayout.Label("Description", GUILayout.Width(100));
        ability.desc_ability = GUILayout.TextField(ability.desc_ability);
        GUILayout.EndHorizontal();

        // Base
        foldBase = EditorGUILayout.Foldout(foldBase, "Base");
        if (foldBase)
        {
            base.OnInspectorGUI();
        }
    }
}