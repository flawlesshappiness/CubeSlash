using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(Area), menuName = "Game/" + nameof(Area), order = 1)]
public class Area : ScriptableObject
{
    [Header("PROPERTIES")]
    public int index_level_min;

    [Header("ENEMIES")]
    public EnemySettings boss;
    public List<AreaEnemyInfo> enemies;

    [Header("FOG")]
    public Color color_fog = Color.white;
    public Color color_bg = Color.white;
    public AnimationCurve ac_alpha_fog;

    [Header("CAMERA")]
    public float camera_size;

    [Header("PARALLAX")]
    [Range(0, 1)] public float parallax_min;
    [Range(0, 1)] public float parallax_max;
    public List<BackgroundLayer> background_layers = new List<BackgroundLayer>();

    [Header("VIGNETTE")]
    public Gradient vignette_gradient;

    [Header("OBJECTS")]
    public List<SpawnObjectInfo> spawn_objects = new List<SpawnObjectInfo>();

    private void OnValidate()
    {
        for (int i = 0; i < background_layers.Count; i++)
        {
            var layer = background_layers[i];
            layer.name = $"{i}";
        }

        parallax_min = Mathf.Min(parallax_min, parallax_max);
        parallax_max = Mathf.Max(parallax_min, parallax_max);
    }
}

[System.Serializable]
public class SpawnObjectInfo
{
    public SpawnObject prefab;
    public float delay;
    public int count;
}

[System.Serializable]
public class AreaEnemyInfo
{
    public EnemySettings enemy;
    public float chance;
    public int max;
}