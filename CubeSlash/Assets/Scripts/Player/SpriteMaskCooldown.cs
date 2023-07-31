using UnityEngine;

public class SpriteMaskCooldown : MonoBehaviour
{
    [SerializeField] private Transform pivot_cooldown;
    [SerializeField] private SpriteMask sprite_mask;

    public void SetCooldown(float t)
    {
        if (pivot_cooldown == null) return;

        var tv = t == 0 ? t : Mathf.Lerp(0.1f, 1f, t);
        pivot_cooldown.transform.localScale = new Vector3(1, tv);
    }

    public void SetSprite(Sprite sprite)
    {
        sprite_mask.sprite = sprite;
    }
}