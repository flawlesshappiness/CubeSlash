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

    private CustomCoroutine cr_spawning;

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

        cr_spawning = CoroutineController.Instance.Run(SpawningCr(), "EnemySpawning");
    }

    public void StopSpawning()
    {
        CoroutineController.Instance.Kill(cr_spawning);
    }

    private IEnumerator SpawningCr()
    {
        while(!IsFinishedSpawning())
        {
            if (enemies_active.Count < CountActiveEnemyMax)
            {
                if (enemies_inactive.Count == 0)
                {
                    ExtendEnemyPool(COUNT_ENEMY_POOL_EXTEND);
                }
                else
                {
                    SpawnEnemy(GetPositionOutsideCamera());
                }
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
        var enemy = enemies_inactive.Pop();
        enemies_active.Add(enemy);
        enemy.gameObject.SetActive(true);
        enemy.transform.position = position;

        var settings = Level.Current.enemies.Where(e => enemies_to_spawn[e.enemy] > 0).ToArray().Random().enemy;
        enemies_to_spawn[settings]--;
        enemy.Initialize(settings);
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
    public void OnEnemyKilled(Enemy enemy)
    {
        enemies_active.Remove(enemy);
        enemies_inactive.Add(enemy);
    }

    public void KillActiveEnemies()
    {
        foreach(var enemy in enemies_active.ToList())
        {
            enemy.Kill();
        }
    }

    public bool AllEnemiesKilled()
    {
        return EnemiesLeft() == 0;
    }

    public int EnemiesLeft()
    {
        var count = 0;
        enemies_to_spawn.ToList().ForEach(kvp => count += kvp.Value);
        return count;
    }
    #endregion
}
