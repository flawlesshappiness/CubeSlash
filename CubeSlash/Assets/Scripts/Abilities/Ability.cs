using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Ability : MonoBehaviourExtended
{
    [Header("PROPERTIES")]
    public Type type;

    [Header("UI")]
    public string name_ability;
    [TextArea] public string desc_ability;
    public Sprite sprite_icon;
    public List<AbilityVariable> variables = new List<AbilityVariable>();
    public List<AbilityModifierEffects> modifier_effects = new List<AbilityModifierEffects>();

    public enum Type { DASH, SPLIT, CHARGE, EXPLODE }
    public Ability[] Modifiers { get; protected set; } = new Ability[ConstVars.COUNT_MODIFIERS];
    public Player Player { get; set; }
    public bool IsPressed { get; set; }
    public bool Equipped { get; set; }
    public bool IsModifier { get; set; }
    public bool IsActive { get { return Equipped && !IsModifier; } }
    public float TimeCooldownStart { get; private set; }
    public float TimeCooldownEnd { get; private set; }
    public float TimeCooldownLeft { get { return OnCooldown ? TimeCooldownEnd - Time.time : 0f; } }
    public bool OnCooldown { get { return Time.time < TimeCooldownEnd; } }
    public bool InUse { get; protected set; }
    public float CooldownPercentage { get { return (Time.time - TimeCooldownStart) / (TimeCooldownEnd - TimeCooldownStart); } }

    private Dictionary<string, AbilityValue> values = new Dictionary<string, AbilityValue>();

    // Values
    public float Cooldown { get; private set; }

    private void OnValidate()
    {
        // Add modifier effects
        var types = System.Enum.GetValues(typeof(Type)).Cast<Type>().ToList();
        foreach(var type in types)
        {
            if(!modifier_effects.Any(e => e.type == type))
            {
                modifier_effects.Add(new AbilityModifierEffects { type = type });
            }
        }

        // Add variables
        if(!variables.Any(v => v.name == "Cooldown"))
        {
            variables.Add(new AbilityVariable
            {
                name = "Cooldown",
                text_display = "$s cooldown",
                type_display = AbilityVariable.DisplayType.FLOAT,
                type_value = AbilityVariable.ValueType.FLOAT,
                can_edit_name = false,
            });
        }
    }

    public virtual void InitializeFirstTime() { }
    public virtual void OnValuesApplied() 
    {
        Cooldown = GetFloatValue("Cooldown");
    }

    #region APPLY
    public void ApplyActive()
    {
        ResetValues();
        ApplyUpgrades();
        ApplyModifiers();
        //
        OnValuesApplied();
    }

    public void ResetValues()
    {
        values.Clear();
        foreach (var v in variables)
        {
            var value = new AbilityValue(v);
            values.Add(v.name, value);
        }
    }

    private void ApplyUpgrades()
    {
        UpgradeController.Instance.GetUnlockedUpgrades().ForEach(info => 
        {
            foreach(var effect in info.upgrade.effects)
            {
                var value = values[effect.variable.name];
                value.AddValue(effect.variable);
            }
        });
    }

    private void ApplyModifiers()
    {
        Modifiers.Where(m => m != null).ToList().ForEach(m => 
        { 
            // Apply modifier variables
        });
    }
    #endregion
    #region INPUT
    public virtual void Pressed()
    {
        IsPressed = true;
    }

    public virtual void Released()
    {
        IsPressed = false;
    }
    #endregion
    #region EQUIP
    public void Equip()
    {
        Equipped = true;
    }

    public void Unequip()
    {
        Equipped = false;
        IsModifier = false;

        for (int i = 0; i < Modifiers.Length; i++)
        {
            var modifier = Modifiers[i];
            if (modifier == null) continue;
            modifier.Unequip();
            Modifiers[i] = null;
        }
    }
    #endregion
    #region COOLDOWN
    public void StartCooldown() => StartCooldown(GetFloatValue("Cooldown"));

    public void StartCooldown(float time)
    {
        InUse = false;
        TimeCooldownStart = Time.time;
        TimeCooldownEnd = TimeCooldownStart + time * Player.Instance.GlobalCooldownMultiplier;
    }
    #endregion
    #region MODIFIER
    public void SetModifier(Ability ability, int idx)
    {
        // Unquip prev
        var prev = Modifiers[idx];
        if (prev)
        {
            prev.IsModifier = false;
            prev.Equipped = false;
        }

        // Equip cur
        Modifiers[idx] = ability;
        if (ability)
        {
            ability.IsModifier = true;
            ability.Equipped = true;
        }
    }

    public bool HasModifier(Type type)
    {
        foreach(var modifier in Modifiers)
        {
            if (modifier != null && modifier.type == type)
                return true;
        }
        return false;
    }

    public T GetModifier<T>(Type type) where T : Ability
    {
        return (T)Modifiers.FirstOrDefault(m => m != null && m.type == type);
    }
    #endregion
    #region VALUES
    public int GetIntValue(string name) => values[name].GetIntValue();
    public float GetFloatValue(string name) => values[name].GetFloatValue();
    public bool GetBoolValue(string name) => values[name].GetBoolValue();
    public void AddValue(string name, int value) => values[name].AddValue(value);
    public void AddValue(string name, float value) => values[name].AddValue(value);
    public void AddValue(string name, bool value) => values[name].AddValue(value);
    #endregion
}
