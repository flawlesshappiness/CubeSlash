using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StatCollection))]
public class StatCollectionEditor : Editor
{
    private StatCollection collection;

    private void OnEnable()
    {
        collection = target as StatCollection;
    }

    public override void OnInspectorGUI()
    {
        GUIHelper.DrawAssetSaveButton(collection);

        collection.id = EditorGUILayout.TextField("ID: ", collection.id);

        DrawDivider();

        EditorGUI.BeginChangeCheck();
        foreach(var stat in collection.stats.ToList())
        {
            DrawStat(stat, out var clicked_remove);

            if (clicked_remove)
            {
                collection.stats.Remove(stat);
            }
        }
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(collection);
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(GUILayout.Button(GUIHelper.GetTexture(GUIHelper.GUITexture.PLUS), GUILayout.Width(30), GUILayout.Height(30)))
        {
            collection.stats.Add(new StatParameter());
            EditorUtility.SetDirty(collection);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void DrawStat(StatParameter stat, out bool clicked_remove)
    {
        clicked_remove = false;

        //  Name
        GUILayout.BeginHorizontal();
        GUILayout.Label("Name:", GUILayout.Width(100));
        GUI.enabled = stat.can_edit_name;
        stat.name = EditorGUILayout.TextField(stat.name);
        GUI.enabled = true;

        GUI.enabled = stat.can_edit_name;
        if (GUILayout.Button(GUIHelper.GetTexture(GUIHelper.GUITexture.MINUS), GUILayout.Width(20), GUILayout.Height(20)))
        {
            clicked_remove = true;
        }
        GUI.enabled = true;

        GUILayout.EndHorizontal();

        //  Display text
        GUILayout.BeginHorizontal();
        GUILayout.Label("Text:", GUILayout.Width(100));
        if (!stat._editor_toggle_preview)
        {
            stat.text_display = EditorGUILayout.TextField(stat.text_display);
        }
        else
        {
            GUI.enabled = false;
            EditorGUILayout.TextField(stat.GetDisplayString(false));
            GUI.enabled = true;
        }
        stat._editor_toggle_preview = GUILayout.Toggle(stat._editor_toggle_preview, "", GUILayout.Width(20));
        GUILayout.EndHorizontal();

        // Value
        GUILayout.BeginHorizontal();

        stat.type_value = (StatParameter.ValueType)EditorGUILayout.EnumPopup(stat.type_value, GUILayout.Width(100));

        if (stat.type_value == StatParameter.ValueType.INT)
        {
            stat.value_int = EditorGUILayout.IntField(stat.value_int);
        }
        else if (stat.type_value == StatParameter.ValueType.FLOAT)
        {
            stat.value_float = EditorGUILayout.FloatField(stat.value_float);
        }
        else if (stat.type_value == StatParameter.ValueType.BOOL)
        {
            stat.value_bool = EditorGUILayout.Toggle(stat.value_bool);
        }
        else
        {
            GUILayout.FlexibleSpace();
        }

        stat.type_display = (StatParameter.DisplayType)EditorGUILayout.EnumPopup(stat.type_display, GUILayout.Width(100));

        GUILayout.EndHorizontal();

        //  Name
        GUILayout.BeginHorizontal();
        GUILayout.Label("High is good:", GUILayout.Width(100));
        GUI.enabled = stat.can_edit_name;
        stat.higher_is_positive = GUILayout.Toggle(stat.higher_is_positive, "", GUILayout.Width(20));
        GUI.enabled = true;

        GUILayout.EndHorizontal();

        DrawDivider();
    }

    private void DrawDivider()
    {
        GUILayout.Space(8);
        GUIHelper.PushColor(Color.black);
        GUILayout.Box("", GUILayout.Height(4), GUILayout.ExpandWidth(true));
        GUIHelper.PopColor();
        GUILayout.Space(8);
    }
}