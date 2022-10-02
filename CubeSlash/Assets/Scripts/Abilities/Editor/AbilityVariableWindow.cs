using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class AbilityVariableWindow : EditorWindow
{
    private Ability ability;
    private Vector2 scrollPosition;

    public static void Show(Ability ability)
    {
        var window = GetWindow<AbilityVariableWindow>();
        window.Initialize(ability);
    }

    public void Initialize(Ability ability)
    {
        this.ability = ability;
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawVariables();
        EditorGUILayout.EndScrollView();
    }

    private void DrawVariables()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUIHelper.CenterLabel($"Variables ({ability.variables.Count})", GUILayout.Height(30));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(GUIHelper.GetTexture(GUIHelper.GUITexture.PLUS), GUILayout.Width(30), GUILayout.Height(30)))
        {
            ability.variables.Add(new AbilityVariable());
        }
        GUILayout.EndHorizontal();

        foreach (var variable in ability.variables.ToList())
        {
            EditorGUI.BeginChangeCheck();
            DrawVariable(variable);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(ability);
            }
        }
    }

    private void DrawVariable(AbilityVariable variable)
    {
        // Preview
        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();
        GUIHelper.PushColor(Color.grey);
        var ali_prev = GUI.skin.box.alignment;
        GUI.skin.box.alignment = TextAnchor.MiddleCenter;
        var text = variable.GetDisplayString(false);
        GUILayout.Box(text, GUILayout.Height(30));
        GUI.skin.box.alignment = ali_prev;
        GUIHelper.PopColor();

        GUILayout.FlexibleSpace();
        GUI.enabled = variable.can_edit_name;
        if (GUILayout.Button(GUIHelper.GetTexture(GUIHelper.GUITexture.MINUS), GUILayout.Width(30), GUILayout.Height(30)))
        {
            ability.variables.Remove(variable);
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        //  Name
        GUILayout.BeginHorizontal();
        GUILayout.Label("Name:", GUILayout.Width(100));
        GUI.enabled = variable.can_edit_name;
        variable.name = EditorGUILayout.TextField(variable.name);
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        //  Display text
        GUILayout.BeginHorizontal();
        GUILayout.Label("Text:", GUILayout.Width(100));
        variable.text_display = EditorGUILayout.TextField(variable.text_display);
        GUILayout.EndHorizontal();

        // Value
        GUILayout.BeginHorizontal();

        variable.type_value = (AbilityVariable.ValueType)EditorGUILayout.EnumPopup(variable.type_value, GUILayout.Width(100));

        if (variable.type_value == AbilityVariable.ValueType.INT)
        {
            variable.value_int = EditorGUILayout.IntField(variable.value_int);
        }
        else if (variable.type_value == AbilityVariable.ValueType.FLOAT)
        {
            variable.value_float = EditorGUILayout.FloatField(variable.value_float);
        }
        else if(variable.type_value == AbilityVariable.ValueType.BOOL)
        {
            variable.value_bool = EditorGUILayout.Toggle(variable.value_bool);
        }
        else
        {
            GUILayout.FlexibleSpace();
        }

        variable.type_display = (AbilityVariable.DisplayType)EditorGUILayout.EnumPopup(variable.type_display, GUILayout.Width(100));

        GUILayout.EndHorizontal();
    }
}