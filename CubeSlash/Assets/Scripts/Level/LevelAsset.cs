using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Level/LevelAsset", order = 1)]
public class LevelAsset : ScriptableObject
{
    public float duration;

    [Header("ENEMY")]
    public float frequency_spawn_enemy;
    public int count_enemy_active;
    public List<Enemy> enemies = new List<Enemy>();
    public List<Enemy> bosses = new List<Enemy>();

    [Header("EXPERIENCE")]
    public float frequency_spawn_experience;
    public int count_experience_active;

    [Header("BACKGROUND")]
    public Color color_fog = Color.white;
    public Color color_bg = Color.white;
    public AnimationCurve ac_alpha_fog;
    [Range(0, 1)] public float parallax_min;
    [Range(0, 1)] public float parallax_max;
    public List<BackgroundLayer> background_layers = new List<BackgroundLayer>();

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

        for (int i = 0; i < background_layers.Count; i++)
        {
            var layer = background_layers[i];
            layer.name = $"{i}";

            foreach(var o in layer.objects)
            {
                o.name = o.item != null ? o.item.name : "MISSING PREFAB";
            }
        }

        parallax_min = Mathf.Min(parallax_min, parallax_max);
        parallax_max = Mathf.Max(parallax_min, parallax_max);
    }

    [System.Serializable]
    public class Enemy
    {
        [HideInInspector] public string name;
        public EnemySettings enemy;
        public float chance;
        public int max;
    }

    [System.Serializable]
    public class BackgroundLayer
    {
        [HideInInspector] public string name;
        public float size_sprite_min;
        public float size_sprite_max;
        public float count_sprite;
        public List<Sprite> sprites = new List<Sprite>();
        public List<ParticleSystem> particles = new List<ParticleSystem>();
        public List<BackgroundObject<GameObject>> objects = new List<BackgroundObject<GameObject>>();
    }

    [System.Serializable]
    public class BackgroundObject<T>
    {
        [HideInInspector] public string name;
        public T item;
        public int count;
        public float size_min;
        public float size_max;
    }
}
