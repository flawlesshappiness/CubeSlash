using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour, IInitializable
{
    public static EnemyController Instance { get; private set; }

    private Enemy prefab_enemy;
    private List<Enemy> enemies_active = new List<Enemy>();
    private List<Enemy> enemies_inactive = new List<Enemy>();
    private Dictionary<EnemySettings, int> enemies_to_spawn = new Dictionary<EnemySettings, int>();
    private Dictionary<EnemySettings, int> bosses_to_spawn = new Dictionary<EnemySettings, int>();

    private CustomCoroutine cr_spawning;

    public System.Action OnEnemyKilled { get; set; }
    public System.Action<Enemy> OnEnemySpawned { get; set; }
    public List<Enemy> ActiveEnemies { get { return enemies_active.ToList(); } }
    private int CountActiveEnemyMax { get { return Level.Current.count_enemy_active; } }

    private const int COUNT_ENEMY_POOL_EXTEND = 20;

    public void Initialize()
    {
        Instance = this;
        prefab_enemy = Resources.Load<Enemy>("Prefabs/Entities/Enemy");
    }

    #region SPAWNING
    public void StartSpawning()
    {
        enemies_to_spawn.Clear();
        foreach(var enemy in Level.Current.enemies)
        {
            enemies_to_spawn.Add(enemy.enemy, enemy.count_total);
        }

        bosses_to_spawn.Clear();
        foreach(var boss in Level.Current.bosses)
        {
            bosses_to_spawn.Add(boss.enemy, boss.count_total);
        }

        cr_spawning = CoroutineController.Instance.Run(SpawningCr(), "EnemySpawning");
    }

    public void StopSpawning()
    {
        CoroutineController.Instance.Kill(cr_spawning);
    }

    private IEnumerator SpawningCr()
    {
        foreach (var kvp in bosses_to_spawn)
        {
            for (int i = 0; i < kvp.Value; i++)
            {
                SpawnEnemy(kvp.Key, GetPositionOutsideCamera());
            }
        }
        bosses_to_spawn.Clear();

        while (!IsFinishedSpawning())
        {
            if (enemies_active.Count < CountActiveEnemyMax)
            {
                SpawnEnemy(GetPositionOutsideCamera());
            }
            yield return null;
        }
    }

    private bool IsFinishedSpawning()
    {
        return !enemies_to_spawn.Any(kvp => kvp.Value > 0);
    }

    private Enemy SpawnEnemy(Vector3 position)
    {
        var settings = Level.Current.enemies.Where(e => enemies_to_spawn[e.enemy] > 0).ToArray().Random().enemy;
        enemies_to_spawn[settings]--;
        return SpawnEnemy(settings, position);
    }

    private Enemy SpawnEnemy(EnemySettings settings, Vector3 position)
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
        var center = GetPositionOutsideCamera();
        for (int i = 0; i < 3; i++)
        {
            var pos = center + Random.insideUnitCircle.normalized.ToVector3() * Random.Range(0.5f, 2f);
            var enemy = SpawnEnemy(pos);
        }
    }

    private Vector3 GetPositionOutsideCamera()
    {
        var dir = Random.insideUnitCircle.normalized.ToVector3();
        var dist = CameraController.Instance.Width * 0.5f * Random.Range(1.2f, 2.0f);
        return CameraController.Instance.Camera.transform.position.SetZ(0) + dir * dist;
    }
    #endregion
    #region POOL
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
    #endregion
    #region ENEMY
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

    public void KillAllEnemies()
    {
        foreach(var key in enemies_to_spawn.Keys.ToList())
        {
            enemies_to_spawn[key] = 0;
        }
        KillActiveEnemies();
    }

    public bool AllEnemiesKilled()
    {
        return EnemiesLeft() == 0;
    }

    public int EnemiesLeft()
    {
        var count = enemies_active.Count;
        enemies_to_spawn.ToList().ForEach(kvp => count += kvp.Value);
        return count;
    }
    #endregion
}
