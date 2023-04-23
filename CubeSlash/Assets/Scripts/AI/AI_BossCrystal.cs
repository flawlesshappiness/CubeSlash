using System.Collections;
using UnityEngine;

public class AI_BossCrystal : BossAI
{
    [SerializeField] private EnemySettings settings_eye;

    private int enemies_to_spawn;
    private int enemies_to_kill;

    private Coroutine cr_spawn;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);

        enemies_to_spawn = 10;
        enemies_to_kill = enemies_to_spawn;

        cr_spawn = StartCoroutine(SpawnCr());
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        StopCoroutine(cr_spawn);
    }

    private IEnumerator SpawnCr()
    {
        while(enemies_to_spawn > 0)
        {
            yield return new WaitForSeconds(2f);
            SpawnEye();
            enemies_to_spawn--;
        }
    }

    private void SpawnEye()
    {
        var position = Player.Instance.transform.position + new Vector3(20, 0);
        var e = EnemyController.Instance.SpawnEnemy(settings_eye, position);
        e.OnDeath += OnEyeDeath;
    }

    private void OnEyeDeath()
    {
        enemies_to_kill--;

        if(enemies_to_kill <= 0)
        {
            Self.Kill();
        }
    }
}