using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : Body
{
    [SerializeField] public SpriteRenderer spr_main;
    [SerializeField] public BodySkeleton skeleton;

    public Color base_color = Color.white;

    public List<Bodypart> Bodyparts { get; private set; } = new List<Bodypart>();

    public Sprite GetBodySprite() => spr_main.sprite;
    public void SetBodySprite(Sprite sprite) => spr_main.sprite = sprite;

    public void ClearBodyparts()
    {
        foreach(var bdp in Bodyparts)
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
            LogController.Instance.LogMessage($"PlayerBody.CreateAbilityBodypart; Failed to create BodypartAbility with type {info.type_bodypart}");
        }

        return bdpa;
    }

    public Bodypart CreateBodypart(BodypartType type)
    {
        var left = InstantiateBodypart(type);
        left.BoneSide = Bodypart.Side.Left;

        var right = InstantiateBodypart(type);
        right.BoneSide = Bodypart.Side.Right;

        left.CounterPart = right;
        right.CounterPart = left;

        right.gameObject.SetActive(false);
        right.SetMirrored(true);

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
        var bdp = BodypartController.Instance.CreateBodypart(type);
        bdp.transform.parent = transform;
        bdp.transform.localScale = Vector3.one;
        bdp.Skeleton = skeleton;
        bdp.SetVariation(0);
        bdp.Initialize();

        Bodyparts.Add(bdp);

        return bdp;
    }
}