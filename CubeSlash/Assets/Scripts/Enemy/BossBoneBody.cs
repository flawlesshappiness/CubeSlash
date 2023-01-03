using UnityEngine;

public class BossBoneBody : EnemyBody
{
    [SerializeField] public ParticleSystem ps_teleport;
    [SerializeField] public Transform[] pivots_wall;
}