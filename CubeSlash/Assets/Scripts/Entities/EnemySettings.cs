using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySettings", menuName = "Enemy/Settings", order = 1)]
public class EnemySettings : ScriptableObject
{
    [Min(1)]public int health;
    public bool boss;

    [Header("CHARACTER")]
    public Character character;
    public Vector3 size = Vector3.one;

    [Header("AI")]
    public EntityAI ai;
}
