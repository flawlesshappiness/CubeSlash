using UnityEngine;
using UnityEngine.UI;

public class RadialMenuLayout : LayoutGroup
{
    [SerializeField][Min(0)] private float distance;
    [SerializeField][Min(0)] private Vector2 size;
    [SerializeField] private bool uniform_rotation;
    [SerializeField] private float angle_offset;

    private Vector2[] positions = new Vector2[0];

    private int ChildCount { get { return rectChildren.Count; } }

    private const float ARC = 360;

    public float AngleOffset => angle_offset;

    public override void SetLayoutVertical()
    {
        var angle_delta = ChildCount < 2 ? 0 : ARC / ChildCount;
        var arc_max = ChildCount < 2 ? 0 : ARC;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            var child = rectChildren[i];
            var position = positions[i];
            SetChildAlongAxis(child, 0, position.x, size.x);
            SetChildAlongAxis(child, 1, position.y, size.y);

            if (uniform_rotation)
            {
                child.transform.rotation = Quaternion.identity;
            }
            else
            {
                child.transform.rotation =
                    Quaternion.AngleAxis(ChildCount < 2 ? 0 : 180f, Vector3.forward) *
                    Quaternion.AngleAxis(arc_max * 0.5f, Vector3.forward) *
                    Quaternion.AngleAxis(-angle_delta * i, Vector3.forward);
            }
        }
    }

    public override void SetLayoutHorizontal()
    {
    }

    public override void CalculateLayoutInputVertical()
    {
        positions = new Vector2[ChildCount];
        var center = new Vector2(GetStartOffset(0, 0), GetStartOffset(1, 0));
        var offset = new Vector2(size.x * 0.5f, size.y * 0.5f);
        var angle_delta = ChildCount < 2 ? 0 : 360f / ChildCount;

        for (int i = 0; i < positions.Length; i++)
        {
            var dir3 =
                Quaternion.AngleAxis(angle_delta * i + angle_offset, Vector3.forward) *
                Vector3.up;
            var dir = new Vector2(dir3.x, dir3.y);
            positions[i] = center - offset + dir * distance;
        }
    }
}