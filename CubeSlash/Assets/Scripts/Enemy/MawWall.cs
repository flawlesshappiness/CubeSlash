using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class MawWall : Obstacle
{
    [SerializeField] private Collider2D collider, trigger;
    [SerializeField] private Transform pivot_sprites;
    [SerializeField] private List<SpriteRenderer> tooth_sprs = new List<SpriteRenderer>();
    [SerializeField] private List<Transform> dud_transforms = new List<Transform>();
    [SerializeField] private SortingGroup sorting_group;
    [SerializeField] private ParticleSystem ps_death;

    public int SortingOrder { set { sorting_group.sortingOrder = value; } }

    public int Layer { get; set; }

    private void Start()
    {
        UpdateDecoration();
    }

    private void UpdateDecoration()
    {
        tooth_sprs.ForEach(spr => spr.enabled = false);

        if (Random.Range(0f, 1f) < 0.5f)
        {
            SetRandomTooth();
        }
    }

    private void SetRandomTooth()
    {
        var tooth = tooth_sprs.Random();
        tooth.enabled = true;
    }

    public void SetLayer(int layer)
    {
        Layer = layer;

        var isBackground = layer != 0;
        hurts = !isBackground;
        enemy_ai_ignore = true;
        collider.enabled = !isBackground;
        trigger.enabled = !isBackground;
        SortingOrder = -layer + (isBackground ? -5 : 0);

        var t_layer = GetLayerTValue();
        var color = GetColor();
        foreach (SpriteRenderer spr in pivot_sprites.GetComponentsInChildren<SpriteRenderer>())
        {
            spr.color = color;
        }
    }

    public float GetLayerTValue()
    {
        var max_layer = 4;
        var t_layer = Layer == 0 ? 0 : Mathf.Lerp(0.7f, 1f, (float)Layer / max_layer);
        return t_layer;
    }

    public Color GetColor()
    {
        var t_layer = GetLayerTValue();
        return Color.Lerp(Color.white, Color.black, t_layer);
    }

    public void Hide()
    {
        foreach (SpriteRenderer spr in pivot_sprites.GetComponentsInChildren<SpriteRenderer>())
        {
            if (!spr.enabled) continue;
            spr.color = Color.black;
        }
    }

    public Coroutine AnimateAppear()
    {
        var lerps = new List<Lerp>();
        var end = GetColor();
        foreach (SpriteRenderer spr in pivot_sprites.GetComponentsInChildren<SpriteRenderer>())
        {
            if (!spr.enabled) continue;
            var lerp = Lerp.Color(spr, 2f, Color.black, end);
            lerps.Add(lerp);
        }

        return StartCoroutine(WaitForLerpsCr());
        IEnumerator WaitForLerpsCr()
        {
            foreach (var lerp in lerps)
            {
                yield return lerp;
            }
        }
    }

    public Transform GetDudTransform()
    {
        var spr = tooth_sprs
            .Where(spr => !spr.enabled)
            .ToList()
            .Random();

        var idx = tooth_sprs.IndexOf(spr);

        return dud_transforms[idx];
    }

    public void Kill()
    {
        SoundController.Instance.PlayGroup(SoundEffectType.sfx_enemy_death);

        ps_death.Duplicate()
            .Parent(GameController.Instance.world)
            .Play()
            .Destroy(10);

        Destroy(gameObject);
    }
}