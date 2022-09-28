using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Upgrade))]
public class UpgradeEditor : Editor
{
    private Upgrade upgrade;
    private UpgradeDatabase db_upgrade;
    private AbilityDatabase db_ability;

    private void OnEnable()
    {
        upgrade = target as Upgrade;
        db_upgrade = AssetDatabase.LoadAssetAtPath<UpgradeDatabase>($"Assets/Resources/Databases/{nameof(UpgradeDatabase)}.asset");
        db_ability = AssetDatabase.LoadAssetAtPath<AbilityDatabase>($"Assets/Resources/Databases/{nameof(AbilityDatabase)}.asset");
    }

    public override void OnInspectorGUI()
    {
        // Icon
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        upgrade.icon = (Sprite)EditorGUILayout.ObjectField(upgrade.icon, typeof(Sprite), false, GUILayout.Height(60), GUILayout.Width(60));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        // ID
        GUILayout.BeginHorizontal();
        GUILayout.Label("ID", GUILayout.Width(100));
        upgrade.id = GUILayout.TextField(upgrade.id);
        GUILayout.EndHorizontal();

        // Name
        GUILayout.BeginHorizontal();
        GUILayout.Label("Name", GUILayout.Width(100));
        upgrade.name = GUILayout.TextField(upgrade.name);
        GUILayout.EndHorizontal();

        // Ability
        GUILayout.BeginHorizontal();
        GUILayout.Label("Ability", GUILayout.Width(100));
        upgrade.ability = (Ability.Type)EditorGUILayout.EnumPopup(upgrade.ability);
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

        var ability = db_ability.GetAbility(upgrade.ability);
        foreach (var effect in upgrade.effects.ToList())
        {
            EditorGUI.BeginChangeCheck();
            DrawEffect(ability, effect, () => upgrade.effects.Remove(effect));
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(upgrade);
            }
        }
    }

    public static void DrawEffect(Ability ability, Upgrade.Effect effect, System.Action onDelete)
    {
        GUILayout.BeginHorizontal();

        var variable = ability.variables.FirstOrDefault(v => v.name == effect.variable.name) ??
            (ability.variables.Count > 0 ? ability.variables[0] : null);

        // Preview
        GUILayout.FlexibleSpace();
        var type_color = effect.type_effect == Upgrade.Effect.TypeEffect.POSITIVE ? ColorPalette.Type.HIGHLIGHT : ColorPalette.Type.WRONG;
        GUIHelper.PushColor(ColorPalette.Main.Get(type_color));
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
            var variables = ability.variables.Select(v => v.name.ToString()).ToArray();
            var idx_variable = ability.variables.IndexOf(variable);

            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            idx_variable = EditorGUILayout.Popup(idx_variable, variables);
            if (EditorGUI.EndChangeCheck())
            {
                variable = ability.variables[idx_variable];
            }

            effect.variable.name = variable.name;
            effect.variable.text_display = variable.text_display;
            effect.variable.type_display = variable.type_display;
            effect.variable.type_value = variable.type_value;

            // Value
            if (effect.variable.type_value == AbilityVariable.ValueType.INT)
            {
                effect.variable.value_int = EditorGUILayout.IntField(effect.variable.value_int);
            }
            else if (effect.variable.type_value == AbilityVariable.ValueType.FLOAT)
            {
                effect.variable.value_float = EditorGUILayout.FloatField(effect.variable.value_float);
            }
            else if(effect.variable.type_value == AbilityVariable.ValueType.BOOL)
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
}