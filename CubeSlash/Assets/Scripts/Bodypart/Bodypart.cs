using System.Linq;
using UnityEngine;

public class Bodypart : MonoBehaviour
{
    [SerializeField] public Transform pivot_mirror, pivot_animation, pivot_scale;
    [SerializeField] private GameObject hover, selected;
    [SerializeField] public SpriteRenderer spr_base;
    [SerializeField] private GameObject[] variations;

    public BodypartInfo Info { get; set; }
    public BodySkeleton Skeleton { get; set; }
    public Bodypart CounterPart { get; set; }
    public BodypartSavaData SaveData { get; set; }
    public int VariationIndex { get; set; }
    public float Position { get; private set; }
    public float Size { get; private set; }
    public bool Mirrored { get; private set; }

    public enum Side { Left, Right };
    public Side BoneSide { get; set; }

    public void Initialize()
    {
        SetHover(false);
        SetSelected(false);
    }

    public void SetPosition(float t, bool is_counter_part = false)
    {
        Position = t;

        var position = Skeleton.GetBonePosition(t);
        var side = BoneSide == Side.Left ? position.left : position.right;
        var angle = Vector3.SignedAngle(side.localNormal, Vector3.up, Vector3.back);
        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        var show_counterpart = !position.is_top_or_bottom;

        gameObject.SetActive(!is_counter_part || show_counterpart);

        transform.localPosition = side.localPosition;
        transform.localRotation = rotation;
        transform.localScale = Vector3.one;

        if (SaveData != null)
        {
            SaveData.position = t;
        }

        if (!is_counter_part)
        {
            CounterPart.SetPosition(t, is_counter_part: true);
        }
    }

    public void SetSize(float t, bool is_counter_part = false)
    {
        Size = t;

        pivot_scale.localScale = Vector3.one * Mathf.Lerp(1f, 2f, t);

        if (SaveData != null)
        {
            SaveData.size = t;
        }

        if (!is_counter_part)
        {
            CounterPart.SetSize(t, is_counter_part: true);
        }
    }

    public void SetVariation(int i)
    {
        var list = variations.ToList();
        list.ForEach(v => v.SetActive(false));
        VariationIndex = Mathf.Clamp(i, 0, list.Count - 1);
        var v = variations[VariationIndex];
        v.SetActive(true);
    }

    public void SetMirrored(bool mirrored, bool is_counter_part = false)
    {
        Mirrored = mirrored;
        var x = mirrored ? -1 : 1;
        pivot_mirror.localScale = new Vector3(x, 1, 1);

        if (!is_counter_part)
        {
            if (SaveData != null)
            {
                SaveData.mirrored = Mirrored;
            }

            CounterPart.SetMirrored(!mirrored, is_counter_part: true);
        }
    }

    public void ToggleMirror()
    {
        SetMirrored(!Mirrored);
    }

    public void SetHover(bool hover)
    {
        if (this.hover != null)
        {
            this.hover.SetActive(hover);
        }
    }

    public void SetSelected(bool selected)
    {
        if (this.selected != null)
        {
            this.selected.SetActive(selected);
        }
    }

    public void SetBaseColor(Color color)
    {
        spr_base.color = color;
    }
}