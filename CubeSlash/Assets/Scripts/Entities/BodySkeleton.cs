using System.Collections.Generic;
using UnityEngine;

public class BodySkeleton : MonoBehaviour
{
    public float height;
    public float offset;
    [Range(0f, 1f)]
    public float t_test;
    public List<float> bones = new List<float>();

    public Vector3 LocalCenter { get { return transform.localPosition; } }
    public Vector3 Up { get { return Vector3.up; } }
    public Vector3 Right { get { return Vector3.right; } }
    public Vector3 OffsetPosition { get { return Up * offset; } }
    public Vector3 Top { get { return LocalCenter + Up * HalfHeight + OffsetPosition; } }
    public Vector3 Bottom { get { return LocalCenter - Up * HalfHeight + OffsetPosition; } }
    public float HalfHeight { get { return height * 0.5f; } }

    private void OnDrawGizmos()
    {
        var i_max = bones.Count - 1;
        for (int i = 0; i < bones.Count; i++)
        {
            Gizmos.color = Color.white;
            var t = (float)i / i_max;
            t = t == 0 ? 0.001f : t == 1 ? 0.999f : t;
            var p = GetBonePosition(t);

            Gizmos.DrawLine(p.left.position, p.right.position);

            if (i < bones.Count - 1)
            {
                var t_next = (float)(i + 1) / i_max;
                t_next = t_next == 1 ? 0.999f : t_next;
                var p_next = GetBonePosition(t_next);

                Gizmos.DrawLine(p.left.position, p_next.left.position);
                Gizmos.DrawLine(p.right.position, p_next.right.position);
            }
        }

        var p_test = GetBonePosition(t_test);

        Gizmos.DrawSphere(p_test.left.position, 0.03f);
        Gizmos.DrawSphere(p_test.right.position, 0.03f);

        Gizmos.DrawLine(p_test.left.position, p_test.left.position + p_test.left.normal * 0.2f);
        Gizmos.DrawLine(p_test.right.position, p_test.right.position + p_test.right.normal * 0.2f);
    }

    public BonePosition GetBonePosition(float t)
    {
        var position = new BonePosition();
        position.is_top_or_bottom = t == 0 || t == 1;

        if(position.is_top_or_bottom)
        {
            var y = offset + (t == 0 ? 0 : height);
            position.left.localPosition = new Vector3(0, y);
            position.right.localPosition = new Vector3(0, y);
        }
        else
        {
            t = 1f - Mathf.Clamp01(t);
            var i_max = bones.Count - 1;
            var i_lower = (int)Mathf.Lerp(0, i_max, t);
            var i_upper = Mathf.Clamp(i_lower + 1, 0, i_max);
            var t_lower = (float)i_lower / i_max;
            var t_upper = (float)i_upper / i_max;
            var t_local = t == 0 ? 0 : t == 1 ? 1 : (t - t_lower) / (t_upper - t_lower);

            //Debug.Log($"({t} - {t_lower}) / ({t_upper} - {t_lower}) = {t_local}");

            var y = height * (1f - t) + offset;
            var w_upper = bones[i_upper];
            var w_lower = bones[i_lower];
            var w = Mathf.Lerp(w_upper, w_lower, 1f - t_local);

            position.left.localPosition = new Vector3(w, y);
            position.right.localPosition = new Vector3(-w, y);
        }

        // Transform
        var center = transform.position;
        var rotation = transform.rotation;

        position.left.position = center + rotation * position.left.localPosition;
        position.right.position = center + rotation * position.right.localPosition;

        position.left.localNormal = position.left.localPosition.normalized;
        position.right.localNormal = position.right.localPosition.normalized;

        position.left.normal = (position.left.position - center).normalized;
        position.right.normal = (position.right.position - center).normalized;

        return position;
    }
}

public struct BonePosition
{
    public BoneSide left;
    public BoneSide right;
    public bool is_top_or_bottom;
}

public struct BoneSide
{
    public Vector3 position;
    public Vector3 localPosition;
    public Vector3 normal;
    public Vector3 localNormal;
}