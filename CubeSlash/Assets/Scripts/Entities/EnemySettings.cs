using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySettings", menuName = "Enemy/Settings", order = 1)]
public class EnemySettings : ScriptableObject
{
    [Header("CHARACTER")]
    public Character character;
    public float speed_move;
    public float speed_turn;
    public Vector3 size = Vector3.one;
    public EnemySettings parasite;

    [Header("AI")]
    public EntityAI ai;
}
