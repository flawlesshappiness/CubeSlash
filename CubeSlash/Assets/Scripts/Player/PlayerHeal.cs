using UnityEngine;

public class PlayerHeal : MonoBehaviour
{
    public float KillValue { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.heal_kill_value).ModifiedValue.float_value; } }
    public float MaxValue { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.heal_max_value).ModifiedValue.float_value; } }

    public Player Player { get { return Player.Instance; } }

    public float ValuePercent { get { return _value / MaxValue; } }

    private float _value;
    private bool _is_full;

    public System.Action<float> OnValueChanged;
    public System.Action<float> OnPercentChanged;
    public System.Action OnHeal;
    public System.Action OnHealFailed;
    public System.Action OnFull;

    private void Start()
    {
        Player.onEnemyKilled += OnEnemyKilled;
    }

    public void Clear()
    {
        SetValue(0);
    }

    public void Press()
    {
        TryHeal();
    }

    public bool IsManaFull() => _value >= MaxValue;

    public bool CanHeal() => Player.Health.HasHealth(HealthPoint.Type.EMPTY);

    public void TryHeal()
    {
        var has_mana = IsManaFull();
        var can_heal = CanHeal();

        if (!has_mana || !can_heal)
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
        SetValue(0);

        SoundController.Instance.Play(SoundEffectType.sfx_gain_health);
    }

    public void SetValue(float value)
    {
        _value = Mathf.Clamp(value, 0, MaxValue);

        if (!_is_full && _value == MaxValue)
        {
            _is_full = true;
            SoundController.Instance.Play(SoundEffectType.sfx_ui_energy_full);
            OnFull?.Invoke();
        }
        else if (_value < MaxValue)
        {
            _is_full = false;
        }

        OnValueChanged?.Invoke(_value);
        OnPercentChanged?.Invoke(ValuePercent);
    }

    public void SetMax() => SetValue(MaxValue);

    private void OnEnemyKilled()
    {
        if (_value >= MaxValue) return;

        SetValue(_value + KillValue);
    }
}