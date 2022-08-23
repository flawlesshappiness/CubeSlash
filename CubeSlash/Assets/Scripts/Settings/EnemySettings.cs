using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySettings", menuName = "Game/Enemy/Settings", order = 1)]
public class EnemySettings : CharacterSettings
{
    [Header("AI")]
    public EntityAI ai;
}
