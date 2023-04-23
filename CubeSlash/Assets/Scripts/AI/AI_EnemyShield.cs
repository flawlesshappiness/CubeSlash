using System.Collections;
using UnityEngine;

public class AI_EnemyShield : EnemyAI
{
    [SerializeField] private ParticleSystem ps_shield;
    [SerializeField] private ParticleSystem ps_break;

    private Vector3 pos_player_prev;

    private int state = 0;
    private float time_invincible;

    private Coroutine cr_unshield;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        state = 0;
        Self.InvincibleLock.AddLock(nameof(AI_EnemyShield));
        Self.OnHit += OnHit;
    }

    private void FixedUpdate()
    {
        if (state == 1)
        {
            MoveToStop();
        }
        else
        {
            pos_player_prev = IsPlayerAlive() ? Player.Instance.transform.position : pos_player_prev;
            MoveTowards(pos_player_prev);
            TurnTowards(pos_player_prev);
        }
    }

    private void OnHit()
    {
        if (Time.time < time_invincible) return;
        SetState(state + 1);
    }

    protected virtual void SetState(int state)
    {
        this.state = state;

        if(state != 2)
        {
            Self.Rigidbody.mass = Self.Settings.mass;
        }

        if (state == 1)
        {
            Body.animator_main.SetBool("shielded", true);
            var duration = 0.2f;
            time_invincible = Time.time + duration;
            Self.Rigidbody.mass = 100f;
            cr_unshield = StartCoroutine(UnshieldCr());
            StartCoroutine(PhaseOutCr(duration));

            ps_shield.Play();
            SoundController.Instance.Play(SoundEffectType.sfx_enemy_crystal_shield);
        }
        else if (state == 2)
        {
            Body.animator_main.SetTrigger("broken");
            var duration = 0.2f;
            time_invincible = Time.time + duration;
            StopCoroutine(cr_unshield);
            StartCoroutine(PhaseOutCr(duration));

            ps_break.Play();
            SoundController.Instance.Play(SoundEffectType.sfx_enemy_crystal_break);
        }
        else if (state > 2)
        {
            Self.InvincibleLock.RemoveLock(nameof(AI_EnemyShield));
        }
    }

    private IEnumerator UnshieldCr()
    {
        yield return new WaitForSeconds(8f);
        Unshield();
    }

    protected void Unshield()
    {
        if(cr_unshield != null) StopCoroutine(cr_unshield);
        Body.animator_main.SetBool("shielded", false);
        SetState(0);

        SoundController.Instance.Play(SoundEffectType.sfx_enemy_crystal_unshield);
    }

    private IEnumerator PhaseOutCr(float delay)
    {
        Self.HitLock.AddLock(nameof(AI_EnemyShield));
        yield return new WaitForSeconds(delay);
        Self.HitLock.RemoveLock(nameof(AI_EnemyShield));
    }
}