using UnityEngine;

public class AbilityItem : Item
{
    [Header("ABILITY ITEM")]
    [SerializeField] private Transform t_spr_upper;
    [SerializeField] private Transform t_spr_lower;

    private void OnEnable()
    {
        Lerp.Euler(t_spr_upper, 8f, 360f, Lerp.Axis.Z).Loop();
        Lerp.Euler(t_spr_lower, 4f, -360f, Lerp.Axis.Z).Loop();

        Lerp.Scale(t_spr_upper, 0.5f, Vector3.one * 0.75f).Curve(Lerp.Curve.SMOOTHERSTEP).Loop().Oscillate();
        Lerp.Scale(t_spr_lower, 1.0f, Vector3.one * 0.75f).Curve(Lerp.Curve.SMOOTHERSTEP).Loop().Oscillate();
    }

    protected override void Collect()
    {
        base.Collect();
        GameController.Instance.OnPlayerGainAbility();
    }

    public override void Despawn()
    {
        base.Despawn();
        ItemController.Instance.OnAbilityItemDespawn(this);
    }
}