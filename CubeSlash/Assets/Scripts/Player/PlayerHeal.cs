using UnityEngine;

public class PlayerHeal : MonoBehaviour
{
    public float KillValue { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.heal_kill_value).ModifiedValue.float_value; } }
    public float MaxValue { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.heal_max_value).ModifiedValue.float_value; } }
    public float Cooldown { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.heal_cooldown).ModifiedValue.float_value; } }

    public Player Player { get { return Player.Instance; } }

    public float Percentage { get { return ((Time.time + _kill_reduc) - _start_time) / (Cooldown * Player.GlobalCooldownMultiplier); } }
    public bool IsFull => _is_full;

    private float _start_time;
    private bool _is_full;
    private float _kill_reduc;

    public System.Action OnHeal;
    public System.Action OnHealFailed;
    public System.Action OnFull;

    private void Start()
    {
        Player.onEnemyKilled += OnEnemyKilled;
    }

    private void Update()
    {
        if (_is_full) return;
        if (Percentage < 1) return;

        SetFull();

        if (GameController.Instance.IsGameStarted)
        {
            SoundController.Instance.Play(SoundEffectType.sfx_ui_energy_full);
        }
    }

    public void Clear()
    {
        SetFull();
    }

    public void Press()
    {
        TryHeal();
    }

    public bool CanHeal() => Player.Health.HasHealth(HealthPoint.Type.EMPTY);

    public void TryHeal()
    {
        var can_heal = CanHeal();

        if (!IsFull || !can_heal)
        {
            SoundController.Instance.Play(SoundEffectType.sfx_ability_cooldown);
            OnHealFailed?.Invoke();
            return;
        }

        Heal();
        OnHeal?.Invoke();
    }

    public void Heal()
    {
        Player.AddHealth(HealthPoint.Type.FULL);
        SoundController.Instance.Play(SoundEffectType.sfx_gain_health);
        StartCooldown();
    }

    public void SetFull()
    {
        _is_full = true;
        OnFull?.Invoke();
    }

    public void StartCooldown()
    {
        _is_full = false;
        _kill_reduc = 0;
        _start_time = Time.time;
    }

    private void OnEnemyKilled()
    {
        if (IsFull) return;
        _kill_reduc += Mathf.Abs(KillValue);
    }
}