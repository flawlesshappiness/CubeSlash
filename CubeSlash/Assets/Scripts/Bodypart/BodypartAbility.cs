using UnityEngine;

public class BodypartAbility : Bodypart
{
    [Header("BodypartAbility")]
    [SerializeField] private SpriteMaskCooldown cooldown;

    [Range(0, 1)]
    public float priority_position;

    private BodypartAbility CounterPartAbility { get { return CounterPart as BodypartAbility; } }

    public void SetCooldown(float t, bool is_counter_part = false)
    {
        cooldown.SetCooldown(t);
        if (!is_counter_part) CounterPartAbility.SetCooldown(t, true);
    }
}