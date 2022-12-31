using UnityEngine;

public class ExperienceItem : Item
{
    [Header("EXPERIENCE")]
    [SerializeField] private SpriteRenderer spr;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Color c_plant;
    [SerializeField] private Color c_meat;

    private ExperienceType type_experience;

    public override void Initialize()
    {
        base.Initialize();
        trail.enabled = true;
    }

    public override void Despawn()
    {
        base.Despawn();
        trail.enabled = false;
        ItemController.Instance.OnExperienceDespawned(this);
    }

    protected override void Collect()
    {
        base.Collect();
        Player.Instance.CollectExperience(type_experience);
        ItemController.Instance.CollectExperience();
    }

    public void SetMeat()
    {
        SetColor(c_meat);
        type_experience = ExperienceType.MEAT;
    }
    
    public void SetPlant()
    {
        SetColor(c_plant);
        type_experience = ExperienceType.PLANT;
    }

    private void SetColor(Color c)
    {
        spr.color = c;
        trail.material.color = c;
    }
}