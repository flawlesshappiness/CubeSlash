using UnityEngine;

public class BossAnglerBody : BossBody
{
    [Header("ANGLER")]
    [SerializeField] private Transform lamp_target;

    public Transform GetLampTarget() => lamp_target;
}