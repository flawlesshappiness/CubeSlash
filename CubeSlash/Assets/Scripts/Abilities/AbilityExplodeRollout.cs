using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityExplodeRollout : Ability
{
    public float Cooldown { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.rollout_cooldown).ModifiedValue.float_value; } }
    public float ExplosionRadius { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.rollout_radius).ModifiedValue.float_value; } }
    public float ExplosionRadiusDelta { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.rollout_radius_delta).ModifiedValue.float_value; } }
    public float ExplosionOffset { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.rollout_offset).ModifiedValue.float_value; } }
    public float ExplosionDelay { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.rollout_delay).ModifiedValue.float_value; } }
    public int ExplosionCount { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.rollout_count).ModifiedValue.int_value; } }
    public int ExplosionLines { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.rollout_line_count).ModifiedValue.int_value; } }
    public bool BehindEnabled { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.rollout_behind).ModifiedValue.bool_value; } }
    public bool ZapEnabled { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.rollout_zap).ModifiedValue.bool_value; } }
    public bool MinisEnabled { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.rollout_minis).ModifiedValue.bool_value; } }

    public override float GetBaseCooldown() => Cooldown;

    public override Dictionary<string, string> GetStats()
    {
        var stats = base.GetStats();
        stats.Add("Cooldown", Cooldown.ToString("0.00"));
        stats.Add("Explosions", ExplosionCount.ToString("0.00"));
        stats.Add("Next radius", ExplosionRadiusDelta.ToString("0.00"));
        stats.Add("Next offset", ExplosionOffset.ToString("0.00"));
        return stats;
    }

    public override void Trigger()
    {
        try
        {
            base.Trigger();
            SelfKnockback();
            TriggerExplosions();
            StartCooldown();
        }
        catch (Exception e)
        {
            LogController.LogException(e);
        }
    }

    private void TriggerExplosions()
    {
        var directions = AbilitySplit.GetSplitDirections(ExplosionLines, 90f, Player.MoveDirection);
        foreach (var direction in directions)
        {
            TriggerExplosion(direction);
        }

        if (BehindEnabled)
        {
            var back_directions = AbilitySplit.GetSplitDirections(ExplosionLines, 90f, -Player.MoveDirection);
            foreach (var direction in back_directions)
            {
                TriggerExplosion(direction);
            }
        }
    }

    private void TriggerExplosion(Vector3 direction)
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var delay = ExplosionDelay;
            var count = ExplosionCount;
            var offset = ExplosionOffset;
            var radius = ExplosionRadius;
            var radius_delta = ExplosionRadiusDelta;
            var start = Player.transform.position + direction.normalized * radius;
            for (int i = 0; i < count; i++)
            {
                var r = radius + radius_delta * i;
                var p = start + direction.normalized * offset * i;
                Explode(r, p);
                yield return new WaitForSeconds(delay);
            }
        }
    }

    private void Explode(float radius, Vector3 position)
    {
        AbilityExplode.Explode(position, radius, 0, OnHit);

        if (MinisEnabled)
        {
            var mini_radius = radius * 0.4f;
            var mini_range = radius * 2;
            AbilityExplode.ExplodeMinis(this, position, 3, mini_radius, mini_range);
        }
    }

    private void OnHit(IKillable k)
    {
        try
        {
            if (ZapEnabled)
            {
                Zap(k);
            }
        }
        catch (Exception e)
        {
            LogController.LogException(e);
        }
    }

    private void Zap(IKillable k)
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var position = k.GetPosition();

            AbilityChain.CreateImpactPS(position);

            yield return new WaitForSeconds(0.2f);

            var info = new AbilityChain.ChainInfo
            {
                center = position,
                radius = 7f,
                chains_left = 1,
                chain_strikes = 1,
                initial_strikes = 1,
                hits = new List<IKillable> { k }
            };

            var success = AbilityChain.TryChainToTarget(info);
        }
    }

    private void SelfKnockback()
    {
        var force = 500f;
        Player.Knockback(-Player.MoveDirection * force, false, false);
    }
}