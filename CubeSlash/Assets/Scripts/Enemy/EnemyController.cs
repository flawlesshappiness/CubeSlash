using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static LevelAsset;

public class EnemyController : Singleton
{
    public static EnemyController Instance { get { return Instance<EnemyController>(); } }

    private Enemy prefab_enemy;
    private List<Enemy> enemies_active = new List<Enemy>();
    private List<Enemy> enemies_inactive = new List<Enemy>();

    public System.Action OnEnemyKilled { get; set; }
    public System.Action<Enemy> OnEnemySpawned { get; set; }
    public List<Enemy> ActiveEnemies { get { return enemies_active.ToList(); } }

    private const int COUNT_ENEMY_POOL_EXTEND = 20;

    private bool has_spawned_boss = true;

    protected override void Initialize()
    {
        prefab_enemy = Resources.Load<Enemy>("Prefabs/Entities/Enemy");
        GameController.Instance.OnNextLevel += OnNextLevel;
    }

    private void Update()
    {
        if (!GameController.Instance.IsGameStarted) return;
        if (GameController.Instance.IsGameEnded) return;
        SpawnUpdate();
    }

    private void OnNextLevel()
    {
        has_spawned_boss = false;
    }

    #region SPAWNING
    private float time_spawn;
    private void SpawnUpdate()
    {
        if (!has_spawned_boss)
        {
            has_spawned_boss = true;
            SpawnBosses();
        }

        if (Time.time < time_spawn) return;
        if (Level.Current.enemies.Count == 0) return;
        if (enemies_active.Count >= Level.Current.count_enemy_active) return;
        time_spawn = Time.time + Level.Current.frequency_spawn_enemy;
        SpawnRandomEnemy(CameraController.Instance.GetPositionOutsideCamera());
    }

    private List<Enemy> SpawnBosses()
    {
        var enemies = new List<Enemy>();

        foreach (var boss in Level.Current.bosses)
        {
            var is_final_level = Level.Current.is_final_level;

            var enemy = SpawnEnemy(boss.enemy, CameraController.Instance.GetPositionOutsideCamera());
            enemies.Add(enemy);
            enemy.OnDeath += () =>
            {
                if (is_final_level)
                {
                    GameController.Instance.Win();
                }
                else
                {
                    for (int i = 0; i < 25; i++)
                    {
                        var experience = ItemController.Instance.SpawnExperience(enemy.transform.position);
                        experience.Initialize();
                        experience.SetMeat();

                        experience.transform.position = enemy.transform.position + Random.insideUnitCircle.ToVector3() * enemy.Settings.size * 0.5f;
                    }
                }
            };
        }

        return enemies;
    }

    private Enemy SpawnRandomEnemy(Vector3 position)
    {
        var random = new WeightedRandom<EnemySettings>();
        foreach(var e in Level.Current.enemies)
        {
            var count = enemies_active.Count(x => x.Settings == e.enemy);
            if(count < e.max || e.max <= 0)
            {
                random.AddElement(e.enemy, e.chance);
            }
        }

        if(random.Count == 0)
        {
            return null;
        }

        var settings = random.Random();
        var enemy = SpawnEnemy(settings, position);
        enemy.OnDeath += () => EnemyDeathSpawnMeat(enemy);

        return enemy;
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

    public void EnemyDeathSpawnMeat(Enemy enemy)
    {
        var experience = ItemController.Instance.SpawnExperience(enemy.transform.position);
        experience.Initialize();
        experience.SetMeat();
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

    public void RemoveActiveEnemies()
    {
        foreach(var enemy in enemies_active.ToList())
        {
            enemy.gameObject.SetActive(false);
            EnemyKilled(enemy);
        }
    }
    #endregion
}
