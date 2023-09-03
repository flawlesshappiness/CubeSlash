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
    private List<AreaEnemyInfo> enemies_unlocked = new List<AreaEnemyInfo>();

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
        enemies_unlocked.AddRange(area.enemies);

        if (cr_spawn_boss != null) StopCoroutine(cr_spawn_boss);
        var boss_spawn_delay = GameSettings.Instance.area_duration * GameSettings.Instance.time_boss_spawn;
        cr_spawn_boss = StartCoroutine(SpawnBossCr(area.boss, boss_spawn_delay));
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
        enemies_unlocked.Clear();
    }

    private void OnMainMenu()
    {
        RemoveActiveEnemies();
    }

    #region SPAWNING
    public float GetSpawnFrequencyDifficulty()
    {
        var settings = GameSettings.Instance;
        var difficulty = DifficultyController.Instance.DifficultyValue;
        return settings.enemy_freq_difficulty.Evaluate(difficulty);
    }

    public float GetSpawnFrequencyGame()
    {
        if (IsFinalBossActive) return 1.2f;
        var settings = GameSettings.Instance;
        var diff = DifficultyController.Instance.DifficultyValue;
        var max_area_count = settings.area_count_difficulty.Evaluate(diff);
        var max_game_duration = max_area_count * settings.area_duration;
        var current_game_duration = Time.time - SessionController.Instance.CurrentData.time_start;
        var t_game_duration = current_game_duration / max_game_duration;
        var freq_game = settings.enemy_freq_game.Evaluate(t_game_duration);
        return freq_game;
    }

    public float GetSpawnFrequency()
    {
        var freq_difficulty = GetSpawnFrequencyDifficulty();
        var freq_game = GetSpawnFrequencyGame();
        return Mathf.Max(0.1f, freq_game + freq_difficulty);
    }

    public int GetSpawnCount()
    {
        var settings = GameSettings.Instance;
        var diff = DifficultyController.Instance.DifficultyValue;
        var max_area_count = settings.area_count_difficulty.Evaluate(diff);
        var max_game_duration = max_area_count * settings.area_duration;
        var current_game_duration = Time.time - SessionController.Instance.CurrentData.time_start;
        var t_game_duration = current_game_duration / max_game_duration;
        var count = (int)settings.enemy_count_game.Evaluate(t_game_duration);
        return count;
    }

    private void SpawnUpdate()
    {
        if (!EnemySpawnEnabled) return;
        if (Time.time < time_next_spawn_enemy) return;
        if (enemies_unlocked.Count == 0) return;
        time_next_spawn_enemy += GetSpawnFrequency();
        var count = GetSpawnCount();
        for (int i = 0; i < count; i++)
        {
            SpawnRandomEnemy(CameraController.Instance.GetPositionOutsideCamera());
        }
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
        var random = new WeightedRandom<EnemySettings>();
        foreach (var e in enemies_unlocked)
        {
            var count = enemies_active.Count(x => x.Settings == e.enemy);
            if (count < e.max || e.max <= 0)
            {
                random.AddElement(e.enemy, e.chance);
            }
        }

        if (random.Count == 0)
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

    public List<AreaEnemyInfo> GetEnemiesUnlocked() =>
        enemies_unlocked.ToList();
    #endregion
}
