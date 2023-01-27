using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Level/LevelAsset", order = 1)]
public class LevelAsset : ScriptableObject
{
    public float duration;
    public bool is_final_level;
    public bool show_tutorial;

    [Header("ENEMY")]
    public float frequency_spawn_enemy;
    public int count_enemy_active;
    public List<Enemy> enemies = new List<Enemy>();
    public List<Enemy> bosses = new List<Enemy>();

    [Header("EXPERIENCE")]
    public float frequency_spawn_experience;
    public int count_experience_active;

    [Header("AREA")]
    public Area area;

    [Header("MUSIC")]
    public FMODEventReference bgm;

    private void OnValidate()
    {
        foreach(var enemy in enemies)
        {
            enemy.name = enemy.enemy == null ? "NONE" : enemy.enemy.name + $" ({(enemy.max <= 0 ? "Infinite" : enemy.max.ToString())})";
        }

        foreach (var enemy in bosses)
        {
            enemy.name = enemy.enemy == null ? "NONE" : enemy.enemy.name;
        }
    }

    [System.Serializable]
    public class Enemy
    {
        [HideInInspector] public string name;
        public EnemySettings enemy;
        public float chance;
        public int max;
    }
}
