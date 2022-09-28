using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AbilityModifierEffectsWindow : EditorWindow
{
    private Ability ability;
    private Vector2 scrollPosition;

    public static void Show(Ability ability)
    {
        var window = GetWindow<AbilityModifierEffectsWindow>();
        window.Initialize(ability);
    }

    public void Initialize(Ability ability)
    {
        this.ability = ability;
    }

    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        DrawModifierEffects();
        GUILayout.EndScrollView();
    }

    private void DrawModifierEffects()
    {
        foreach(var effect in ability.modifier_effects)
        {
            DrawModifierEffect(effect);
        }
    }

    private void DrawModifierEffect(AbilityModifierEffects modifier_effect)
    {
        DrawEffects(modifier_effect);

        GUIHelper.PushColor(Color.grey);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
        GUIHelper.PopColor();
    }

    private void DrawEffects(AbilityModifierEffects modifier_effect)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUIHelper.CenterLabel(modifier_effect.type.ToString(), GUILayout.Height(30));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(GUIHelper.GetTexture(GUIHelper.GUITexture.PLUS), GUILayout.Width(30), GUILayout.Height(30)))
        {
            modifier_effect.effects.Add(new Upgrade.Effect());
            EditorUtility.SetDirty(ability);
        }
        GUILayout.EndHorizontal();

        foreach (var effect in modifier_effect.effects.ToList())
        {
            EditorGUI.BeginChangeCheck();
            UpgradeEditor.DrawEffect(ability, effect, () => modifier_effect.effects.Remove(effect));
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(ability);
            }
        }
    }
}