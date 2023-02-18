using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Stat))]
public class StatEditor : Editor
{
    private Stat stat;
    private void OnEnable()
    {
        stat = target as Stat;
    }

    public override void OnInspectorGUI()
    {
        GUIHelper.DrawAssetSaveButton(stat);
        GUIHelper.DrawDatabaseButtons<StatDatabase, Stat>(stat);
        GUILayout.Space(20);
        base.OnInspectorGUI();

        var s_display = stat.GetDisplayString();

        var ali_prev = GUI.skin.label.alignment;
        GUI.skin.box.alignment = TextAnchor.MiddleCenter;
        GUILayout.Box(s_display, GUILayout.Height(30), GUILayout.ExpandWidth(true));
        GUI.skin.box.alignment = ali_prev;
        GUILayout.Space(10);
    }
}