using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : Body
{
    private List<Bodypart> bodyparts = new List<Bodypart>();

    public void ClearBodyparts()
    {
        foreach(var bp in bodyparts)
        {
            Destroy(bp.gameObject);
        }
        bodyparts.Clear();
    }

    public List<Bodypart> CreateBodyparts(Bodypart prefab)
    {
        var bps = new List<Bodypart>();
        var position = skeleton.GetBonePosition(prefab.priority_position);

        Create(position.left);

        if (!position.is_top_or_bottom)
        {
            Create(position.right);
        }

        return bps;

        void Create(BoneSide side)
        {
            var bp = Instantiate(prefab);
            bp.transform.parent = transform;
            bodyparts.Add(bp);

            bp.transform.localPosition = side.localPosition;

            var angle = Vector3.SignedAngle(side.normal, Vector3.up, Vector3.back);
            var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            bp.transform.localRotation = rotation;

            bps.Add(bp);
        }
    }
}