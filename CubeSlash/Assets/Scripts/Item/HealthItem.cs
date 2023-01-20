using UnityEngine;

public class HealthItem : Item
{
    public HealthPoint.Type type;

    protected override void Collect()
    {
        base.Collect();
        Player.Instance.CollectHealth(type);
    }

    public override void Despawn()
    {
        base.Despawn();
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}