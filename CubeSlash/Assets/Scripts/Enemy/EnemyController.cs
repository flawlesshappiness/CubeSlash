using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : Singleton
{
    public static EnemyController Instance { get { return Instance<EnemyController>(); } }

    public EnemyDatabase Database => _database ?? (_database = EnemyDatabase.Load<EnemyDatabase>());
    private EnemyDatabase _database;

    private Enemy prefab_enemy;
    private List<Enemy> enemies_active = new List<Enemy>();
    private List<Enemy> enemies_inactive = new List<Enemy>();
    private List<EnemySettings> normal_enemies_unlocked = new List<EnemySettings>();
    private List<EnemySettings> special_enemies_unlocked = new List<EnemySettings>();

    public bool EnemySpawnEnabled { get; set; } = true;
    public bool IsFinalBossActive { get; set; }
    public System.Action OnEnemyKilled { get; set; }
    public System.Action<Enemy> OnEnemySpawned { get; set; }
    public System.Action<EnemyType> OnBossKilled { get; set; }
    public List<Enemy> ActiveEnemies { get { return enemies_active.ToList(); } }

    private const int COUNT_ENEMY_POOL_EXTEND = 20;

    private Coroutine cr_spawn_boss;
    private float time_next_spawn_enemy;
    private float time_spawn_boss;

    protected override void Initialize()
    {
        prefab_enemy = Resources.Load<Enemy>("Prefabs/Entities/Enemy");
        AreaController.Instance.onNextArea += OnNextArea;
        GameController.Instance.onGameStart += OnGameStart;
        GameController.Instance.onGameEnd += OnGameEnd;
        GameController.Instance.onMainMenu += OnMainMenu;
    }

    private void Update()
    {
        if (!GameController.Instance.IsGameStarted) return;
        if (GameController.Instance.IsGameEnded) return;
        SpawnUpdate();
    }

    private void OnNextArea(Area area)
    {
        // Normal enemies
        foreach (var enemy in area.normal_enemies)
        {
            var settings = Database.Get(enemy);
            normal_enemies_unlocked.Add(settings);
        }

        // Special enemies
        foreach (var enemy in area.special_enemies)
        {
            var settings = Database.Get(enemy);
            special_enemies_unlocked.Add(settings);
        }

        StartSpawnBossTimer();
    }

    private void OnGameStart()
    {
        IsFinalBossActive = false;
        EnemySpawnEnabled = true;
        time_next_spawn_enemy = Time.time;
    }

    private void OnGameEnd()
    {
        // Clear boss
        if (cr_spawn_boss != null)
        {
            StopCoroutine(cr_spawn_boss);
            cr_spawn_boss = null;
        }

        // Clear unlocked enemies
        normal_enemies_unlocked.Clear();
        special_enemies_unlocked.Clear();
    }

    private void OnMainMenu()
    {
        RemoveActiveEnemies();
    }

    #region SPAWNING
    private void SpawnUpdate()
    {
        if (!EnemySpawnEnabled) return;
        if (normal_enemies_unlocked.Count == 0) return;
        if (special_enemies_unlocked.Count == 0) return;
        if (Time.time < time_next_spawn_enemy) return;

        time_next_spawn_enemy += GameSettings.Instance.EnemySpawnFrequency;

        var count = GameSettings.Instance.EnemySpawnCount;
        for (int i = 0; i < count; i++)
        {
            if (ActiveEnemies.Count >= GameSettings.Instance.EnemyMaxCount) return;

            SpawnRandomEnemy(CameraController.Instance.GetPositionOutsideCamera());
        }
    }

    private void StartSpawnBossTimer()
    {
        var area = AreaController.Instance.CurrentArea;
        if (cr_spawn_boss != null) StopCoroutine(cr_spawn_boss);
        var boss_spawn_delay = GameSettings.Instance.area_duration * GameSettings.Instance.time_boss_spawn;
        cr_spawn_boss = StartCoroutine(SpawnBossCr(area.boss, boss_spawn_delay));
    }

    private IEnumerator SpawnBossCr(EnemySettings boss, float delay)
    {
        time_spawn_boss = Time.time + delay;
        while (Time.time < time_spawn_boss)
        {
            yield return null;
        }

        SpawnAreaBoss(boss);
    }

    public void DebugSpawnBoss()
    {
        StartSpawnBossTimer();
        time_spawn_boss = Time.time;
    }

    private Enemy SpawnBoss(EnemySettings boss)
    {
        var enemy = SpawnEnemy(boss, CameraController.Instance.GetPositionOutsideCamera());
        var size_mul = GameSettings.Instance.boss_size_difficulty.Evaluate(DifficultyController.Instance.DifficultyValue);
        enemy.transform.localScale *= size_mul;
        return enemy;
    }

    private Enemy SpawnAreaBoss(EnemySettings boss)
    {
        if (AreaController.Instance.IsFinalArea)
        {
            IsFinalBossActive = true;
        }

        var area = AreaController.Instance.CurrentArea;
        var enemy = SpawnBoss(boss);
        enemy.OnDeath += () =>
        {
            // Win
            if (boss.type == EnemyType.BossMaw)
            {
                GameController.Instance.Win();
            }

            // Exp
            if (!AreaController.Instance.IsFinalArea && boss.type != EnemyType.BossPlant)
            {
                for (int i = 0; i < 25; i++)
                {
                    var experience = ItemController.Instance.SpawnExperience(enemy.transform.position);
                    experience.Initialize();
                    experience.SetMeat();
                    experience.transform.position = enemy.transform.position + Random.insideUnitCircle.ToVector3() * enemy.Settings.size * 0.5f;
                    experience.AnimateCollect();
                }
            }

            // Log
            OnBossKilled?.Invoke(enemy.Settings.type);
        };

        return enemy;
    }

    private Enemy SpawnRandomEnemy(Vector3 position)
    {
        var is_special = Random.Range(0f, 1f) < 0.2f;
        var list = is_special ? special_enemies_unlocked : normal_enemies_unlocked;
        var settings = list.Random();
        var enemy = SpawnEnemy(settings, position);
        enemy.OnDeath += () => EnemyDeathSpawnMeat(enemy);

        return enemy;
    }

    public Enemy SpawnEnemy(EnemySettings settings, Vector3 position)
    {
        if (enemies_inactive.Count == 0)
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
        experience.AnimateCollect();
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

    public void KillActiveEnemies(List<Enemy> enemies_except = null)
    {
        foreach (var enemy in enemies_active.ToList())
        {
            if (enemies_except != null && enemies_except.Contains(enemy)) continue;
            enemy.Kill(false);
        }
    }

    public void RemoveActiveEnemies()
    {
        foreach (var enemy in enemies_active.ToList())
        {
            enemy.gameObject.SetActive(false);
            EnemyKilled(enemy);
        }
    }
    #endregion
}
