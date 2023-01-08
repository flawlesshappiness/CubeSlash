using System.Collections.Generic;
using UnityEngine;

public class BodySkeleton : MonoBehaviour
{
    public float height;
    public float offset;
    [Range(0f, 1f)]
    public float t_test;
    public List<float> bones = new List<float>();
    public List<BoneInfo> boneInfos;

    public Vector3 Center { get { return transform.position; } }
    public Vector3 Up { get { return transform.up; } }
    public Vector3 Right { get { return transform.right; } }
    public Vector3 OffsetPosition { get { return Up * offset; } }
    public Vector3 Top { get { return Center + Up * HalfHeight + OffsetPosition; } }
    public Vector3 Bottom { get { return Center - Up * HalfHeight + OffsetPosition; } }
    public float HalfHeight { get { return height * 0.5f; } }

    private void OnValidate()
    {
        InitializeBones();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(Top, Bottom);

        if(boneInfos != null)
        {
            for (int i = 0; i < boneInfos.Count; i++)
            {
                Gizmos.color = Color.white;
                var info = GetBone(i);
                var left = Center + info.left.localPosition;
                var right = Center + info.right.localPosition;

                if (i > 0)
                {
                    var info_prev = GetBone(i - 1);
                    Gizmos.DrawLine(left, Center + info_prev.left.localPosition);
                    Gizmos.DrawLine(right, Center + info_prev.right.localPosition);
                }

                Gizmos.DrawLine(left, right);
                Gizmos.DrawSphere(Center + info.localPosition, 0.03f);

                // Normals
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(left, left + info.left.normal);
                Gizmos.DrawLine(right, right + info.right.normal);
            }
        }

        var bone_position = GetBonePosition(t_test);
        var p_left = Center + bone_position.left.localPosition;
        var p_right = Center + bone_position.right.localPosition;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(Center + bone_position.upper.localPosition, 0.05f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Center + bone_position.lower.localPosition, 0.05f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(p_left, 0.05f);
        Gizmos.DrawSphere(p_right, 0.05f);

        Gizmos.DrawLine(p_left, p_left + bone_position.left.normal);
        Gizmos.DrawLine(p_right, p_right + bone_position.right.normal);
    }

    private void InitializeBones()
    {
        boneInfos = new List<BoneInfo>();

        var p_delta = height / (bones.Count - 1);
        for (int i = 0; i < bones.Count; i++)
        {
            var b = bones[i];
            var info = new BoneInfo();
            info.width = b;
            info.localPosition = (Up * HalfHeight) + (-Up * p_delta * i) + OffsetPosition;
            info.left.localPosition = info.localPosition - Right * info.width;
            info.right.localPosition = info.localPosition + Right * info.width;

            boneInfos.Add(info);
        }

        // Normals
        for (int i = 0; i < boneInfos.Count; i++)
        {
            var info = GetBone(i);
            if(i == 0)
            {
                info.left.normal = Vector3.up;
                info.right.normal = Vector3.up;
            }
            else if(i == boneInfos.Count - 1)
            {
                info.left.normal = Vector3.down;
                info.right.normal = Vector3.down;
            }
            else
            {
                var prev = GetBone(i - 1);
                var next = GetBone(i + 1);

                var n_prev_left = CalculateNormal(prev, info, b => b.left.localPosition);
                var n_next_left = CalculateNormal(info, next, b => b.left.localPosition);
                info.left.normal = Vector3.Lerp(n_prev_left, n_next_left, 0.5f).normalized;

                var n_prev_right = CalculateNormal(info, prev, b => b.right.localPosition);
                var n_next_right = CalculateNormal(next, info, b => b.right.localPosition);
                info.right.normal = Vector3.Lerp(n_prev_right, n_next_right, 0.5f).normalized;
            }
        }

        Vector3 CalculateNormal(BoneInfo boneA, BoneInfo boneB, System.Func<BoneInfo, Vector3> getPosition)
        {
            var dir = getPosition(boneB) - getPosition(boneA);
            return Vector3.Cross(dir, Vector3.forward).normalized;
        }
    }

    public BoneInfo GetBone(int i)
    {
        if(boneInfos == null)
        {
            InitializeBones();
        }
        return boneInfos[i];
    }

    public int GetBoneCount()
    {
        if(boneInfos == null)
        {
            InitializeBones();
        }
        return boneInfos.Count;
    }

    public BoneInfo GetLowerBone(float t)
    {
        t = 1 - t;
        var count = GetBoneCount() - 1;
        var i = (int)Mathf.Clamp(1 + count * t, 1, count);
        return GetBone(i);
    }

    public BoneInfo GetUpperBone(float t)
    {
        t = 1 - t;
        var count = GetBoneCount() - 1;
        var i = (int)Mathf.Clamp(count * t, 0, count - 1);
        return GetBone(i);
    }

    public BonePositionInfo GetBonePosition(float t)
    {
        if(t == 0 || t == 1)
        {
            var bone = t == 0 ? GetLowerBone(0) : GetUpperBone(1);
            var _info = new BonePositionInfo
            {
                upper = bone,
                lower = bone,
                is_top_or_bottom = true,
            };

            _info.left.localPosition = bone.localPosition;
            _info.right.localPosition = bone.localPosition;
            _info.left.normal = bone.left.normal;
            _info.right.normal = bone.right.normal;

            return _info;
        }

        var upper = GetUpperBone(t);
        var lower = GetLowerBone(t);

        var y_top = Top.y;
        var y_bottom = Bottom.y;
        var y_t = Mathf.Lerp(y_top, y_bottom, 1 - t);
        var y_upper = upper.localPosition.y;
        var y_lower = lower.localPosition.y;
        var t_local = 1 - ((y_t - y_lower) / (y_upper - y_lower));
        //Debug.Log($"({y_t} - {y_lower}) / ({y_upper} - {y_lower}) = {t_local}");

        var info = new BonePositionInfo
        {
            upper = upper,
            lower = lower,
        };

        info.left.localPosition = Vector3.Lerp(upper.left.localPosition, lower.left.localPosition, t_local);
        info.right.localPosition = Vector3.Lerp(upper.right.localPosition, lower.right.localPosition, t_local);

        info.left.normal = Vector3.Lerp(upper.left.normal, lower.left.normal, t_local);
        info.right.normal = Vector3.Lerp(upper.right.normal, lower.right.normal, t_local);

        return info;
    }
}

public class BoneInfo
{
    public Vector3 localPosition;
    public BoneSide left;
    public BoneSide right;
    public float width;
}

public class BonePositionInfo
{
    public BoneInfo upper;
    public BoneInfo lower;
    public BoneSide left;
    public BoneSide right;
    public bool is_top_or_bottom;
}

public struct BoneSide
{
    public Vector3 localPosition;
    public Vector3 normal;
}