using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spr;

    public void SetColor(Color color)
    {
        spr.color = color;
    }
}