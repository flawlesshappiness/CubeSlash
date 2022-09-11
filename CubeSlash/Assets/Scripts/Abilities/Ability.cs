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
    protected float CooldownTime { get; set; }
    public bool OnCooldown { get { return Time.time < TimeCooldownEnd; } }
    public bool InUse { get; protected set; }
    public float CooldownPercentage { get { return (Time.time - TimeCooldownStart) / (TimeCooldownEnd - TimeCooldownStart); } }

    #region APPLY
    public void ApplyActive()
    {
        ResetValues();
        ApplyUpgrades();
        ApplyModifiers();
    }

    private void ApplyUpgrades()
    {
        UpgradeController.Instance.Database.upgrades.Select(data => UpgradeController.Instance.GetUpgrade(data.type))
            .ToList().ForEach(upgrade => ApplyUpgrade(upgrade));
    }

    private void ApplyModifiers()
    {
        Modifiers.Where(m => m != null)
            .ToList().ForEach(m => ApplyModifier(m));
    }

    public virtual void InitializeFirstTime() { }
    public virtual void ResetValues() { }
    public virtual void ApplyUpgrade(Upgrade upgrade) { }
    public virtual void ApplyModifier(Ability modifier) { }
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
    public void StartCooldown() => StartCooldown(CooldownTime);

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
}
