using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviourExtended
{
    [Header("PROPERTIES")]
    public Type type;
    public float cooldown;

    [Header("UI")]
    public string name_ability;
    [TextArea] public string desc_ability;
    public Sprite sprite_icon;

    public enum Type { DASH, SPLIT, CHARGE }
    public bool BlockingMovement { get; protected set; } = false;
    public bool BlockingAbilities { get; protected set; } = false;
    public Ability[] Modifiers { get; protected set; } = new Ability[ConstVars.COUNT_MODIFIERS];

    public Player Player { get; set; }
    public bool Equipped { get; set; }

    public static Ability Create(Type type)
    {
        return type switch
        {
            Type.DASH => Instantiate(Resources.Load<Ability>("Prefabs/Abilities/Dash").gameObject).GetComponent<Ability>(),
            Type.SPLIT => Instantiate(Resources.Load<Ability>("Prefabs/Abilities/Split").gameObject).GetComponent<Ability>(),
            Type.CHARGE => Instantiate(Resources.Load<Ability>("Prefabs/Abilities/Charge").gameObject).GetComponent<Ability>(),
            _ => null,
        };
    }

    public virtual void Pressed()
    {

    }

    public virtual void Released()
    {

    }

    public virtual void EnemyCollision(Enemy enemy)
    {

    }

    public void SetModifier(Ability ability, int idx)
    {
        // Unquip prev
        var prev = Modifiers[idx];
        if (prev)
        {
            prev.Equipped = false;
        }

        // Equip cur
        Modifiers[idx] = ability;
        if (ability)
        {
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
}
