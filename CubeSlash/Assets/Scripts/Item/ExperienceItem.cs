using UnityEngine;

public class ExperienceItem : Item
{
    [Header("EXPERIENCE")]
    [SerializeField] private SpriteRenderer spr;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Color c_plant;
    [SerializeField] private Color c_meat;

    public override void Despawn()
    {
        base.Despawn();
        ItemController.Instance.OnExperienceDespawned(this);
    }

    protected override void Collect()
    {
        base.Collect();
        Player.Instance.CollectExperience();
        ItemController.Instance.CollectExperience();
    }

    public void SetMeat()
    {
        SetColor(c_meat);
    }
    
    public void SetPlant()
    {
        SetColor(c_plant);
    }

    private void SetColor(Color c)
    {
        spr.color = c;
        trail.material.color = c;
    }
}