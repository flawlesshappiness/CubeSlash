using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Upgrade))]
public class UpgradeEditor : Editor
{
    private Upgrade upgrade;

    private void OnEnable()
    {
        upgrade = target as Upgrade;
    }

    public override void OnInspectorGUI()
    {
        GUIHelper.DrawAssetSaveButton(upgrade);
        GUIHelper.DrawDatabaseButtons<UpgradeDatabase, Upgrade>(upgrade);
        base.OnInspectorGUI();
    }

    /*
    public override void OnInspectorGUI()
    {
        GUIHelper.DrawAssetSaveButton(upgrade);

        // Icon
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        EditorGUI.BeginChangeCheck();
        upgrade.icon = (Sprite)EditorGUILayout.ObjectField(upgrade.icon, typeof(Sprite), false, GUILayout.Height(60), GUILayout.Width(60));
        if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(upgrade); }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        // ID
        GUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        GUILayout.Label("ID", GUILayout.Width(100));
        upgrade.id = GUILayout.TextField(upgrade.id);
        if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(upgrade); }

        GUILayout.EndHorizontal();

        // Name
        GUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        GUILayout.Label("Name", GUILayout.Width(100));
        upgrade.name = GUILayout.TextField(upgrade.name);
        if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(upgrade); }

        GUILayout.EndHorizontal();

        // Stat ID
        GUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        GUILayout.Label("Stat ID:", GUILayout.Width(100));
        var stat_ids = db_stat.collections.Select(c => c.id).ToArray();
        var idx_stat_ids = stat_ids.ToList().IndexOf(upgrade.id_stats);
        idx_stat_ids = EditorGUILayout.Popup(idx_stat_ids, stat_ids);
        if (EditorGUI.EndChangeCheck())
        {
            upgrade.id_stats = stat_ids[idx_stat_ids];
            EditorUtility.SetDirty(upgrade);
        }

        GUILayout.EndHorizontal();

        // Effects
        GUILayout.Space(20);
        DrawEffects();

        GUILayout.Space(20);

        if (db_upgrade.upgrades.Contains(upgrade))
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUIHelper.PushColor(Color.Lerp(Color.green, Color.white, 0.4f));
            GUIHelper.CenterLabel("Exists in database", GUILayout.Height(30));
            GUIHelper.PopColor();

            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Remove from database", GUILayout.Width(200), GUILayout.Height(30)))
            {
                db_upgrade.upgrades.Remove(upgrade);
                EditorUtility.SetDirty(db_upgrade);
                AssetDatabase.SaveAssets();
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add to database", GUILayout.Width(200), GUILayout.Height(30)))
            {
                db_upgrade.upgrades.Add(upgrade);
                EditorUtility.SetDirty(db_upgrade);
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
    */
    /*
    private void DrawEffects()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUIHelper.CenterLabel("Effects", GUILayout.Height(30));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(GUIHelper.GetTexture(GUIHelper.GUITexture.PLUS), GUILayout.Width(30), GUILayout.Height(30)))
        {
            upgrade.effects.Add(new Upgrade.Effect());
            EditorUtility.SetDirty(upgrade);
        }
        GUILayout.EndHorizontal();

        var stats = StatCollection.Load(upgrade.id_stats);

        foreach (var effect in upgrade.effects.ToList())
        {
            EditorGUI.BeginChangeCheck();
            DrawEffect(stats, effect, () => upgrade.effects.Remove(effect));
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(upgrade);
            }
        }
    }

    public static void DrawEffect(StatCollection stat_collection, Upgrade.Effect effect, System.Action onDelete)
    {
        GUILayout.BeginHorizontal();

        var variable = 
            stat_collection == null ? null :
            stat_collection.stats.FirstOrDefault(v => v.name == effect.variable.name) ??
            (stat_collection.stats.Count > 0 ? stat_collection.stats[0] : null);

        var db_color = ColorDatabase.Load();

        // Preview
        GUILayout.FlexibleSpace();
        var color = effect.type_effect == Upgrade.Effect.TypeEffect.POSITIVE ? db_color.text_normal.GetColor() : db_color.text_wrong.GetColor();
        GUIHelper.PushColor(color);
        var ali_prev = GUI.skin.box.alignment;
        GUI.skin.box.alignment = TextAnchor.MiddleCenter;
        var text = variable != null ? effect.variable.GetDisplayString(true) : "No variable";
        GUILayout.Box(text, GUILayout.Height(30));
        GUI.skin.box.alignment = ali_prev;
        GUIHelper.PopColor();

        GUILayout.FlexibleSpace();
        if (GUILayout.Button(GUIHelper.GetTexture(GUIHelper.GUITexture.MINUS), GUILayout.Width(30), GUILayout.Height(30)))
        {
            //upgrade.effects.Remove(effect);
            onDelete?.Invoke();
        }
        GUILayout.EndHorizontal();

        // Variable
        if(variable != null)
        {
            var variables = stat_collection.stats.Select(v => v.name.ToString()).ToArray();
            var idx_variable = stat_collection.stats.IndexOf(variable);

            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            idx_variable = EditorGUILayout.Popup(idx_variable, variables);
            if (EditorGUI.EndChangeCheck())
            {
                variable = stat_collection.stats[idx_variable];
            }

            effect.variable.name = variable.name;
            effect.variable.text_display = variable.text_display;
            effect.variable.type_display = variable.type_display;
            effect.variable.type_value = variable.type_value;

            // Value
            if (effect.variable.type_value == StatValue.ValueType.INT)
            {
                effect.variable.value_int = EditorGUILayout.IntField(effect.variable.value_int);
            }
            else if (effect.variable.type_value == StatValue.ValueType.FLOAT)
            {
                effect.variable.value_float = EditorGUILayout.FloatField(effect.variable.value_float);
            }
            else if(effect.variable.type_value == StatValue.ValueType.BOOL)
            {
                effect.variable.value_bool = EditorGUILayout.Toggle(effect.variable.value_bool);
            }
            else
            {
                GUILayout.FlexibleSpace();
            }

            effect.type_effect = (Upgrade.Effect.TypeEffect)EditorGUILayout.EnumPopup(effect.type_effect, GUILayout.Width(100));
            GUILayout.EndHorizontal();
        }
    }
    */
}