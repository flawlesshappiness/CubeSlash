using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySettings", menuName = "Game/Enemy/Settings", order = 1)]
public class EnemySettings : ScriptableObject
{
    public EnemyType type;

    [Header("CHARACTER")]
    public Body body;
    public float linear_acceleration;
    public float linear_velocity;
    public float linear_drag;
    public float angular_acceleration;
    public float angular_velocity;
    public float size = 1;
    public float mass = 1;

    [Header("AI")]
    public EnemyAI ai;
}
