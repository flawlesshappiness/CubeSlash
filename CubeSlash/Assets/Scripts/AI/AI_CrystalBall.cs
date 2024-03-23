using UnityEngine;

public class AI_CrystalBall : AI_SlowMove
{
    public int HealthMax;
    public ParticleSystem ps_crystal_break;

    private int health;

    private CrystalBallBody CrystalBody => Body as CrystalBallBody;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);

        health = HealthMax;

        Self.InvincibleLock.AddLock(nameof(AI_EnemyShield));
        Self.OnHit += OnHit;

        UpdateSprite();
    }

    private void OnHit()
    {
        health--;

        ps_crystal_break.Play();

        if (health > 0)
        {
            UpdateSprite();

            SoundController.Instance.SetGroupVolumeByPosition(SoundEffectType.sfx_enemy_crystal_break, Position);
            SoundController.Instance.PlayGroup(SoundEffectType.sfx_enemy_crystal_break);
        }
        else
        {
            Self.Kill();
        }
    }

    private void UpdateSprite()
    {
        CrystalBody.spr.sprite = GetHealthSprite();
    }

    private Sprite GetHealthSprite()
    {
        var t = health / (float)HealthMax;
        return GetSprite(t);
    }

    private Sprite GetSprite(float t)
    {
        var i_max = CrystalBody.sprite_states.Length - 1;
        var i = (int)Mathf.Clamp(Mathf.Lerp(0, i_max + 1, t), 0, i_max);
        return CrystalBody.sprite_states[i];
    }
}