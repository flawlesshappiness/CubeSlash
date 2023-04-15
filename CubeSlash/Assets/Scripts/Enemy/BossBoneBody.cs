using UnityEngine;

public class BossBoneBody : BossBody
{
    [Header("BONE")]
    [SerializeField] public ParticleSystem ps_teleport;
    [SerializeField] public Transform[] pivots_wall;
}