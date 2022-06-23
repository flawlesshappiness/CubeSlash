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

    public enum Type { DASH, SPLIT, CHARGE }
    public Ability[] Modifiers { get; protected set; } = new Ability[ConstVars.COUNT_MODIFIERS];
    public AbilityVariable[] Variables { get; protected set; } = new AbilityVariable[ConstVars.COUNT_VARIABLES];

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
    public float CooldownPercentage { get { return (Time.time - TimeCooldownStart) / (TimeCooldownEnd - TimeCooldownStart); } }

    public static Ability GetPrefab(Type type)
    {
        return Resources.Load<Ability>(GetPrefabPath(type));
    }

    private static string GetPrefabPath(Type type)
    {
        return "Prefabs/Abilities/" + type.ToString();
    }

    public virtual void InitializeFirstTime()
    {

    }

    public void InitializeActive()
    {
        InitializeValues();
        foreach(var modifier in Modifiers.Where(m => m != null))
        {
            InitializeModifier(modifier);
        }
    }

    public virtual void InitializeValues() { }
    public virtual void InitializeModifier(Ability modifier) { }

    public virtual void Pressed()
    {
        IsPressed = true;
    }

    public virtual void Released()
    {
        IsPressed = false;
    }

    public virtual void EnemyCollision(Enemy enemy)
    {

    }

    #region COOLDOWN
    public void StartCooldown()
    {
        TimeCooldownStart = Time.time;
        TimeCooldownEnd = TimeCooldownStart + CooldownTime;
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
