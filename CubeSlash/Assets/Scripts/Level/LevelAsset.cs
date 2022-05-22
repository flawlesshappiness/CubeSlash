using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Level/LevelAsset", order = 1)]
public class LevelAsset : ScriptableObject
{
    public int count_enemy_active;
    public List<Enemy> enemies = new List<Enemy>();

    private void OnValidate()
    {
        foreach(var enemy in enemies)
        {
            enemy.name = enemy.enemy == null ? "NONE" : enemy.enemy.name;
        }
    }

    [System.Serializable]
    public class Enemy
    {
        [HideInInspector] public string name;
        public EnemySettings enemy;
        public int count_total = 1;
    }
}
