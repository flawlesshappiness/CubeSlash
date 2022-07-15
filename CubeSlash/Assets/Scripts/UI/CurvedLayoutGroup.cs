using UnityEngine;
using UnityEngine.UI;

public class CurvedLayoutGroup : LayoutGroup
{
    [SerializeField] private float angle;
    [SerializeField] private float arc;
    [SerializeField] private bool use_spacing;
    [SerializeField][Min(0)] private float spacing;
    [SerializeField][Min(0)] private float distance;
    [SerializeField][Min(0)] private Vector2 size;
    [SerializeField] private float roll;
    [SerializeField] private bool invert_order;

    private Vector2[] positions = new Vector2[0];

    private int ChildCount { get { return rectChildren.Count; } }

    public override void SetLayoutVertical()
    {
        var angle_delta = use_spacing ? spacing : ChildCount < 2 ? 0 : arc / (ChildCount - 1);

        var arc_max =
            use_spacing ? spacing * (ChildCount - 1) :
            ChildCount < 2 ? 0 : arc;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            var idx = invert_order ? (ChildCount - 1 - i) : i;
            var child = rectChildren[idx];
            var position = positions[i];
            SetChildAlongAxis(child, 0, position.x, size.x);
            SetChildAlongAxis(child, 1, position.y, size.y);

            child.transform.rotation =
                Quaternion.AngleAxis(arc_max * 0.5f, Vector3.forward) * 
                Quaternion.AngleAxis(-angle_delta * i - angle - roll, Vector3.forward);
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

        var up = -Vector3.up;
        var angle_delta = use_spacing ? -spacing : ChildCount < 2 ? 0 : -arc / (ChildCount - 1);

        var arc_max = 
            use_spacing ? spacing * (ChildCount - 1) : 
            ChildCount < 2 ? 0 : arc;

        for (int i = 0; i < positions.Length; i++)
        {
            var dir3 = 
                Quaternion.AngleAxis(angle, Vector3.forward) * 
                Quaternion.AngleAxis(-arc_max * 0.5f, Vector3.forward) * 
                Quaternion.AngleAxis(-angle_delta * i, Vector3.forward) * 
                up;
            var dir = new Vector2(dir3.x, dir3.y);
            positions[i] = center - offset + dir * distance;
        }
    }
}