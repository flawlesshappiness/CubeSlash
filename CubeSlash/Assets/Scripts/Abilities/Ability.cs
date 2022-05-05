using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour
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
    public List<Ability> modifiers { get; private set; } = new List<Ability>();

    public Player Player { get; set; }
    public bool Equipped { get; set; }

    public virtual void Pressed()
    {

    }

    public virtual void Released()
    {

    }

    public virtual void EnemyCollision(Enemy enemy)
    {

    }

    public static Ability Create(Type type)
    {
        return type switch
        {
            Type.DASH => Instantiate(Resources.Load<Ability>("Prefabs/Abilities/Dash").gameObject).GetComponent<Ability>(),
            Type.SPLIT => Instantiate(Resources.Load<Ability>("Prefabs/Abilities/Split").gameObject).GetComponent<Ability>(),
            _ => null,
        };
    }
}
