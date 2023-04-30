using System.Linq;
using UnityEngine;

public class Bodypart : MonoBehaviour
{
    [SerializeField] private GameObject[] variations;

    public BodypartInfo Info { get; set; }
    public BodySkeleton Skeleton { get; set; }
    public Bodypart CounterPart { get; set; }
    public int VariationIndex { get; set; }
    public float Position { get; set; }

    public enum Side { Left, Right };
    public Side BoneSide { get; set; }

    public void SetPosition(float y, bool is_counter_part = false)
    {
        Position = y;

        var position = Skeleton.GetBonePosition(y);
        var side = BoneSide == Side.Left ? position.left : position.right;
        var angle = Vector3.SignedAngle(side.normal, Vector3.up, Vector3.back);
        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        var show_counterpart = !position.is_top_or_bottom;

        gameObject.SetActive(!is_counter_part || show_counterpart);

        transform.localPosition = side.localPosition;
        transform.localRotation = rotation;
        transform.localScale = Vector3.one;

        if (!is_counter_part)
        {
            CounterPart.SetPosition(y, is_counter_part: true);
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
}