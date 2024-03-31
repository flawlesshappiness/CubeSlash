using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : Body
{
    [SerializeField] public SpriteRenderer spr_main;
    [SerializeField] public SpriteMaskCooldown dodge_cooldown;
    [SerializeField] public BodySkeleton skeleton;

    public Color base_color = Color.white;

    public List<Bodypart> Bodyparts { get; private set; } = new List<Bodypart>();

    public Sprite GetBodySprite() => spr_main.sprite;
    public void SetBodySprite(Sprite sprite)
    {
        spr_main.sprite = sprite;
    }

    public void ClearBodyparts()
    {
        foreach (var bdp in Bodyparts)
        {
            Destroy(bdp.gameObject);
        }
        Bodyparts.Clear();
    }

    public BodypartAbility CreateAbilityBodypart(AbilityInfo info)
    {
        var bdp = CreateBodypart(info.type_bodypart);
        var bdpa = bdp as BodypartAbility;
        bdpa.SetPosition(bdpa.priority_position);

        if (bdpa == null)
        {
            LogController.LogMessage($"PlayerBody.CreateAbilityBodypart; Failed to create BodypartAbility with type {info.type_bodypart}");
        }

        return bdpa;
    }

    public Bodypart CreateBodypart(BodypartType type)
    {
        LogController.LogMethod(type.id);

        var left = InstantiateBodypart(type);
        var right = InstantiateBodypart(type);

        left.BoneSide = Bodypart.Side.Left;
        right.BoneSide = Bodypart.Side.Right;

        left.CounterPart = right;
        right.CounterPart = left;

        right.gameObject.SetActive(false);
        right.SetMirrored(true, is_counter_part: true);

        left.SetBaseColor(base_color);
        right.SetBaseColor(base_color);

        return left;
    }

    public void RemoveBodypart(Bodypart part)
    {
        var counter = part.CounterPart;
        Bodyparts.Remove(part);
        Bodyparts.Remove(counter);

        Destroy(part.gameObject);
        Destroy(counter.gameObject);
    }

    private Bodypart InstantiateBodypart(BodypartType type)
    {
        LogController.LogMethod(type.id);

        var bdp = BodypartController.Instance.CreateBodypart(type);
        bdp.transform.parent = transform;
        bdp.transform.localScale = Vector3.one;
        bdp.Skeleton = skeleton;
        bdp.SetVariation(0);
        bdp.Initialize();

        Bodyparts.Add(bdp);

        return bdp;
    }

    public void SetDodgeCooldown(float t)
    {
        dodge_cooldown.SetCooldown(t);
    }
}