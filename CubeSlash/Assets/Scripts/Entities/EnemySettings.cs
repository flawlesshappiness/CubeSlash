using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySettings", menuName = "Game/Enemy/Settings", order = 1)]
public class EnemySettings : ScriptableObject
{
    [Header("CHARACTER")]
    public Character character;
    public float linear_acceleration;
    public float linear_velocity;
    public float angular_acceleration;
    public float angular_velocity;
    public float size = 1;
    public float mass = 1;

    [Header("AI")]
    public EntityAI ai;
}
