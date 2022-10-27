using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Ability : MonoBehaviourExtended
{
    [Header("ABILITY")]
    public AbilityInfo Info;
    public StatCollection Stats;
    public AbilityModifierCollection ModifierUpgrades;

    public enum Type { DASH, SPLIT, CHARGE, EXPLODE }
    public Ability[] Modifiers { get; protected set; } = new Ability[ConstVars.COUNT_MODIFIERS];
    public Player Player { get; set; }
    public bool IsPressed { get; set; }
    public bool Equipped { get; set; }
    public Ability ModifierParent { get; set; }
    public bool IsModifier { get { return ModifierParent != null; } }
    public bool IsActive { get { return Equipped && !IsModifier; } }
    public float TimeCooldownStart { get; private set; }
    public float TimeCooldownEnd { get; private set; }
    public float TimeCooldownLeft { get { return OnCooldown ? TimeCooldownEnd - Time.time : 0f; } }
    public bool OnCooldown { get { return Time.time < TimeCooldownEnd; } }
    public bool InUse { get; protected set; }
    public float CooldownPercentage { get { return (Time.time - TimeCooldownStart) / (TimeCooldownEnd - TimeCooldownStart); } }
    public int CurrentCharges { get; set; }

    private Dictionary<string, StatValue> values = new Dictionary<string, StatValue>();

    // Values
    public float Cooldown { get; protected set; }
    public int Charges { get; protected set; }

    private void OnValidate()
    {
        // Add variables
        if(Stats != null)
        {
            if (!Stats.stats.Any(v => v.name == "Cooldown"))
            {
                Stats.stats.Add(new StatParameter
                {
                    name = "Cooldown",
                    text_display = "$s cooldown",
                    type_display = StatParameter.DisplayType.FLOAT,
                    type_value = StatParameter.ValueType.FLOAT,
                    can_edit_name = false,
                    higher_is_positive = false,
                });
            }

            if (!Stats.stats.Any(v => v.name == "Charges"))
            {
                Stats.stats.Add(new StatParameter
                {
                    name = "Charges",
                    text_display = "$ charges",
                    type_display = StatParameter.DisplayType.INT,
                    type_value = StatParameter.ValueType.INT,
                    can_edit_name = false,
                    higher_is_positive = true,
                });
            }
        }
    }

    public virtual void InitializeFirstTime() { }
    public virtual void OnValuesApplied() 
    {
        Cooldown = GetFloatValue("Cooldown");
        Charges = GetIntValue("Charges");
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
        foreach (var v in Stats.stats)
        {
            var value = new StatValue(v);
            values.Add(v.name, value);
        }
    }

    private void ApplyUpgrades()
    {
        UpgradeController.Instance.GetUnlockedUpgrades()
            .Where(info => info.require_ability && info.type_ability_required == Info.type)
            .ToList().ForEach(info => ApplyEffects(info.upgrade.effects));
    }

    private void ApplyModifiers()
    {
        Modifiers
            .Where(m => m != null)
            .Select(m => ModifierUpgrades.GetModifier(m.Info.type))
            .Where(am => am.upgrade != null)
            .ToList().ForEach(am => ApplyEffects(am.upgrade.effects));
    }

    private void ApplyEffects(List<Upgrade.Effect> effects)
    {
        foreach (var effect in effects)
        {
            var value = values[effect.variable.name];
            value.AddValue(effect.variable);
        }
    }
    #endregion
    #region INPUT
    public virtual void Pressed()
    {
        IsPressed = true;

        var charge = GetModifier<AbilityCharge>(Type.CHARGE);
        if(Info.type == Type.CHARGE)
        {
            // Do nothing
        }
        else if(charge != null)
        {
            charge.BeginCharge();
        }
        else
        {
            Trigger();
        }
    }

    public virtual void Released()
    {
        IsPressed = false;

        var charge = GetModifier<AbilityCharge>(Type.CHARGE);
        if (Info.type == Type.CHARGE)
        {
            // Do nothing
        }
        else if (charge != null)
        {
            if (charge.EndCharge())
            {
                Trigger();
            }
            else
            {
                // Do nothing
            }
        }
    }

    public virtual void Trigger()
    {
        // Trigger ability
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
        ModifierParent = null;

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
        CurrentCharges--;
        if (CurrentCharges <= 0)
        {
            TimeCooldownStart = Time.time;
            TimeCooldownEnd = TimeCooldownStart + time * Player.Instance.GlobalCooldownMultiplier;
            StartCoroutine(WaitForCooldownCr());
        }

        IEnumerator WaitForCooldownCr()
        {
            while(Time.time < TimeCooldownEnd)
            {
                yield return null;
            }

            CurrentCharges = Charges;
        }
    }
    #endregion
    #region MODIFIER
    public void SetModifier(Ability ability, int idx)
    {
        // Unquip prev
        var prev = Modifiers[idx];
        if (prev)
        {
            prev.ModifierParent = null;
            prev.Equipped = false;
        }

        // Equip cur
        Modifiers[idx] = ability;
        if (ability)
        {
            ability.ModifierParent = this;
            ability.Equipped = true;
        }
    }

    public bool HasModifier(Type type)
    {
        foreach(var modifier in Modifiers)
        {
            if (modifier != null && modifier.Info.type == type)
                return true;
        }
        return false;
    }

    public T GetModifier<T>(Type type) where T : Ability
    {
        return (T)Modifiers.FirstOrDefault(m => m != null && m.Info.type == type);
    }
    #endregion
    #region VALUES
    public int GetIntValue(string name) => values[name].GetIntValue();
    public float GetFloatValue(string name) => values[name].GetFloatValue();
    public bool GetBoolValue(string name) => values[name].GetBoolValue();
    public void AddValue(string name, int value) => values[name].AddValue(value);
    public void AddValue(string name, float value) => values[name].AddValue(value);
    public void AddValue(string name, bool value) => values[name].AddValue(value);
    public StatValue GetStat(string name) => values[name];
    #endregion
}
