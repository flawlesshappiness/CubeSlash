using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : Singleton
{
    public static EnemyController Instance { get { return Instance<EnemyController>(); } }

    private Enemy prefab_enemy;
    private List<Enemy> enemies_active = new List<Enemy>();
    private List<Enemy> enemies_inactive = new List<Enemy>();

    public System.Action OnEnemyKilled { get; set; }
    public System.Action<Enemy> OnEnemySpawned { get; set; }
    public List<Enemy> ActiveEnemies { get { return enemies_active.ToList(); } }
    private int CountActiveEnemyMax { get { return Level.Current.count_enemy_active; } }

    private const int COUNT_ENEMY_POOL_EXTEND = 20;

    public override void Initialize()
    {
        prefab_enemy = Resources.Load<Enemy>("Prefabs/Entities/Enemy");
    }

    private void Update()
    {
        SpawnUpdate();
    }

    #region SPAWNING
    private bool spawning;
    private float time_spawn;
    private void SpawnUpdate()
    {
        if (!spawning) return;
        if (Time.time < time_spawn) return;
        time_spawn = Time.time + Level.Current.frequency_spawn;
        SpawnEnemy(CameraController.Instance.GetPositionOutsideCamera());
    }

    public void SetSpawningEnabled(bool enabled)
    {
        this.spawning = enabled;
    }

    private Enemy SpawnEnemy(Vector3 position)
    {
        var random = new WeightedRandom<EnemySettings>();
        foreach(var enemy in Level.Current.enemies)
        {
            random.AddElement(enemy.enemy, enemy.chance);
        }

        var settings = random.Random();
        return SpawnEnemy(settings, position);
    }

    public Enemy SpawnEnemy(EnemySettings settings, Vector3 position)
    {
        if(enemies_inactive.Count == 0)
        {
            ExtendEnemyPool(COUNT_ENEMY_POOL_EXTEND);
        }

        var enemy = enemies_inactive.Pop();
        enemies_active.Add(enemy);
        enemy.gameObject.SetActive(true);
        enemy.transform.position = position;

        enemy.Initialize(settings);

        OnEnemySpawned?.Invoke(enemy);
        
        return enemy;
    }

    private void SpawnCluster()
    {
        var center = CameraController.Instance.GetPositionOutsideCamera();
        for (int i = 0; i < 3; i++)
        {
            var pos = center + Random.insideUnitCircle.normalized.ToVector3() * Random.Range(0.5f, 2f);
            var enemy = SpawnEnemy(pos);
        }
    }
    #endregion
    #region POOL
    private Enemy CreateEnemyForPool()
    {
        var enemy = CreateEnemy();
        enemy.gameObject.SetActive(false);
        enemies_inactive.Add(enemy);
        return enemy;
    }

    private void ExtendEnemyPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CreateEnemyForPool();
        }
    }
    #endregion
    #region ENEMY
    private Enemy CreateEnemy()
    {
        var enemy = Instantiate(prefab_enemy.gameObject).GetComponent<Enemy>();
        enemy.transform.parent = GameController.Instance.world;
        return enemy;
    }

    public void EnemyKilled(Enemy enemy)
    {
        enemies_active.Remove(enemy);
        enemies_inactive.Add(enemy);
        OnEnemyKilled?.Invoke();
    }

    public void KillActiveEnemies()
    {
        foreach(var enemy in enemies_active.ToList())
        {
            enemy.Kill();
        }
    }
    #endregion
}
