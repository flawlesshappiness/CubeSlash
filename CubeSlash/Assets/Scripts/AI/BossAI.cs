using System.Collections;
using System.Linq;
using UnityEngine;

public class BossAI : EnemyAI
{
    protected BossBody Body { get { return Self.Body as BossBody; } }

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);

        Self.IsBoss = true;
        Self.OnDeath += OnDeath;

        if (Body.HasDuds)
        {
            Body.OnDudKilled += OnDudKilled;
        }
    }

    protected virtual void OnDeath()
    {
        SoundController.Instance.Play(SoundEffectType.sfx_enemy_boss_scream);
    }

    private void OnDudKilled(HealthDud dud)
    {
        var count_alive = Body.Duds.Count(dud => !dud.Dead);
        if (count_alive == 0)
        {
            Self.Kill();
        }
    }

    protected void HideAndShowDuds(float duration)
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            HideDuds();
            yield return new WaitForSeconds(duration);
            ShowDuds();
        }
    }

    protected void HideDuds()
    {
        Body.Duds.Where(d => d.IsAlive()).ToList().ForEach(d => d.SetDudActive(false));
    }

    protected void ShowDuds()
    {
        Body.Duds.Where(d => d.IsAlive()).ToList().ForEach(d => d.SetDudActive(true));
    }

    protected void DistableObstaclesInArena(float radius)
    {
        ForeachObstacleInArena(radius, o =>
        {
            o.DisableDestroy();
        });
    }

    protected void ForeachObstacleInArena(float radius, System.Action<Obstacle> action)
    {
        var hits = Physics2D.OverlapCircleAll(Self.transform.position, radius);
        foreach (var hit in hits)
        {
            var obstacle = hit.GetComponentInParent<Obstacle>();
            if (obstacle == null) continue;

            if (!obstacle.is_area_obstacle) continue;

            action?.Invoke(obstacle);
        }
    }
}