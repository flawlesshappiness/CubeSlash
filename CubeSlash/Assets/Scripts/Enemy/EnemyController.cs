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

    public System.Action OnEnemyKilled { get; set; }
    public System.Action<Enemy> OnEnemySpawned { get; set; }
    public List<Enemy> ActiveEnemies { get { return enemies_active.ToList(); } }

    private const int COUNT_ENEMY_POOL_EXTEND = 20;

    private Coroutine cr_spawn_boss;

    protected override void Initialize()
    {
        prefab_enemy = Resources.Load<Enemy>("Prefabs/Entities/Enemy");
        AreaController.Instance.onNextArea += OnNextArea;
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

        var boss_spawn_delay = GameSettings.Instance.area_duration * GameSettings.Instance.time_boss_spawn;
        cr_spawn_boss = StartCoroutine(SpawnBossCr(area.boss, boss_spawn_delay));
    }

    private void OnGameEnd()
    {
        // Clear boss
        if(cr_spawn_boss != null)
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
    private float GetSpawnFrequency()
    {
        var settings = GameSettings.Instance;

        // Game
        var max_game_duration = settings.areas_to_win * settings.area_duration;
        var current_game_duration = Time.time - GameController.Instance.TimeGameStart;
        var t_game_duration = current_game_duration / max_game_duration;
        var freq_game = settings.enemy_freq_game.Evaluate(t_game_duration);

        // Area
        var max_area_duration = settings.area_duration;
        var current_area_duration = Time.time - AreaController.Instance.TimeAreaStart;
        var t_area_duration = current_area_duration / max_area_duration;
        var freq_area = settings.enemy_freq_area.Evaluate(t_area_duration);

        // Endless
        if (AreaController.Instance.IsEndless)
        {
            var max_endless_duration = settings.endless_duration;
            var current_endless_duration = Time.time - AreaController.Instance.TimeEndlessStart;
            var t_endless_duration = current_endless_duration / max_endless_duration;
            freq_game = settings.enemy_freq_endless.Evaluate(t_endless_duration);
        }

        return freq_game + freq_area;
    }

    private float time_spawn;
    private void SpawnUpdate()
    {
        if (Time.time < time_spawn) return;
        if (enemies_unlocked.Count == 0) return;
        //if (enemies_active.Count >= Level.Current.count_enemy_active) return;
        time_spawn = Time.time + GetSpawnFrequency();
        SpawnRandomEnemy(CameraController.Instance.GetPositionOutsideCamera());
    }

    private IEnumerator SpawnBossCr(EnemySettings boss, float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnBoss(boss);
    }

    private Enemy SpawnBoss(EnemySettings boss)
    {
        var is_game_winning = (AreaController.Instance.AreaIndex + 1) == GameSettings.Instance.areas_to_win;

        var enemy = SpawnEnemy(boss, CameraController.Instance.GetPositionOutsideCamera());
        enemy.OnDeath += () =>
        {
            // Win
            if (is_game_winning)
            {
                GameController.Instance.Win();
            }

            // Exp
            for (int i = 0; i < 25; i++)
            {
                var experience = ItemController.Instance.SpawnExperience(enemy.transform.position);
                experience.Initialize();
                experience.SetMeat();

                experience.transform.position = enemy.transform.position + Random.insideUnitCircle.ToVector3() * enemy.Settings.size * 0.5f;
            }
        };

        return enemy;
    }

    private Enemy SpawnRandomEnemy(Vector3 position)
    {
        var random = new WeightedRandom<EnemySettings>();
        foreach(var e in enemies_unlocked)
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
