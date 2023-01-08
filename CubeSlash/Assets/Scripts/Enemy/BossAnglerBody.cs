using UnityEngine;

public class BossAnglerBody : EnemyBody
{
    [SerializeField] private Transform lamp_target;

    public Transform GetLampTarget() => lamp_target;
}