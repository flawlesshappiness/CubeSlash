using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySettings", menuName = "Game/Enemy/Settings", order = 1)]
public class EnemySettings : ScriptableObject
{
    [Header("CHARACTER")]
    public Character character;
    public float speed_max;
    public float speed_acceleration;
    public float speed_turn;
    public Vector3 size = Vector3.one;
    public float mass = 1;
    public EnemySettings parasite;

    [Header("AI")]
    public EntityAI ai;
}
