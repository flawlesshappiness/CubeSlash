using UnityEngine;

public class BodypartAbility : Bodypart
{
    [Header("BodypartAbility")]
    [SerializeField]
    private Transform pivot_cooldown;

    [Range(0, 1)]
    public float priority_position;

    private BodypartAbility CounterPartAbility { get { return CounterPart as BodypartAbility; } }

    public void SetCooldown(float t, bool is_counter_part = false)
    {
        if (pivot_cooldown == null) return;

        var tv = t == 0 ? t : Mathf.Lerp(0.1f, 1f, t);
        pivot_cooldown.transform.localScale = new Vector3(1, tv);

        if (!is_counter_part) CounterPartAbility.SetCooldown(t, true);
    }
}