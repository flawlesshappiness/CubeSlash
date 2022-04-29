using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour, IInitializable
{
    public static EnemyController Instance { get; private set; }

    public bool Spawning { get; set; }

    private Dictionary<Enemy.Type, EnemySettings> settings = new Dictionary<Enemy.Type, EnemySettings>();

    private Enemy prefab_enemy;
    private List<Enemy> enemies_active = new List<Enemy>();
    private List<Enemy> enemies_inactive = new List<Enemy>();

    private const int COUNT_ENEMY_MAX = 15;
    private const int COUNT_ENEMY_POOL_EXTEND = 20;
    private const float CHANCE_SPAWN_CLUSTER = 0.1f;

    public void Initialize()
    {
        Instance = this;
        InitializeSettings();
        prefab_enemy = Resources.Load<Enemy>("Prefabs/Entities/Enemy");
    }

    private void InitializeSettings()
    {
        var assets = Resources.LoadAll<EnemySettings>("Prefabs/Enemies/Settings");
        foreach(var asset in assets)
        {
            if (!settings.ContainsKey(asset.type))
            {
                settings.Add(asset.type, asset);
            }
        }
    }

    private void Update()
    {
        SpawnUpdate();
    }

    private void SpawnUpdate()
    {
        if (!Spawning) return;

        if(enemies_active.Count < COUNT_ENEMY_MAX)
        {
            if (enemies_inactive.Count == 0)
            {
                ExtendEnemyPool(COUNT_ENEMY_POOL_EXTEND);
            }
            else
            {
                if(Random.Range(0f, 1f) < CHANCE_SPAWN_CLUSTER)
                {
                    SpawnCluster();
                }
                else
                {
                    SpawnEnemy(GetPositionOutsideCamera());
                }
            }
        }
    }

    private Enemy SpawnEnemy(Vector3 position)
    {
        var enemy = enemies_inactive.Pop();
        enemies_active.Add(enemy);
        enemy.gameObject.SetActive(true);
        enemy.transform.position = position;
        var type = Random.Range(0f, 1f) < 0.15f ? Enemy.Type.CHONK : Enemy.Type.WEAK;
        enemy.Initialize(settings[type]);
        return enemy;
    }

    private void SpawnCluster()
    {
        var center = GetPositionOutsideCamera();
        for (int i = 0; i < 3; i++)
        {
            var pos = center + Random.insideUnitCircle.normalized.ToVector3() * Random.Range(0.5f, 2f);
            var enemy = SpawnEnemy(pos);
        }
    }

    private Enemy CreateEnemy()
    {
        var enemy = Instantiate(prefab_enemy.gameObject).GetComponent<Enemy>();
        enemy.transform.parent = GameController.Instance.world;
        enemy.gameObject.SetActive(false);
        enemies_inactive.Add(enemy);
        return enemy;
    }

    private void ExtendEnemyPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CreateEnemy();
        }
    }

    public void OnEnemyKilled(Enemy enemy)
    {
        enemies_active.Remove(enemy);
        enemies_inactive.Add(enemy);
    }

    private Vector3 GetPositionOutsideCamera()
    {
        var dir = Random.insideUnitCircle.normalized.ToVector3();
        var dist = CameraController.Instance.Width * 0.5f * Random.Range(1.2f, 2.0f);
        return CameraController.Instance.Camera.transform.position.SetZ(0) + dir * dist;
    }
}
