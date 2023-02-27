using System.Collections;
using UnityEngine;

public abstract class Ability : MonoBehaviourExtended
{
    [Header("ABILITY")]
    public AbilityInfo Info;

    public event System.Action onTrigger;
    public event System.Action onCooldownComplete;

    public enum Type { DASH, SPLIT, CHARGE, EXPLODE, CHAIN, MINES }
    public Player Player { get { return Player.Instance; } }
    public bool IsPressed { get; set; }
    public float TimeCooldownStart { get; private set; }
    public float TimeCooldownEnd { get; protected set; }
    public float TimeCooldownLeft { get { return IsOnCooldown ? TimeCooldownEnd - Time.time : 0f; } }
    public float CooldownPercentage { get { return (Time.time - TimeCooldownStart) / (TimeCooldownEnd - TimeCooldownStart); } }
    public bool IsOnCooldown { get { return Time.time < TimeCooldownEnd; } }
    public bool InUse { get; set; }

    private Coroutine cr_release;

    public virtual void InitializeFirstTime() 
    {
        PlayerValueController.Instance.onValuesUpdated += OnValuesUpdated;
    }

    public virtual void OnValuesUpdated() 
    {
    }

    protected int GetIntValue(StatID id) => PlayerValueController.Instance.GetIntValue(id);
    protected float GetFloatValue(StatID id) => PlayerValueController.Instance.GetFloatValue(id);
    protected bool GetBoolValue(StatID id) => PlayerValueController.Instance.GetBoolValue(id);

    public bool HasModifier(Type modifier) => AbilityController.Instance.HasModifier(Info.type, modifier);

    #region INPUT
    public void ResetInput()
    {
        IsPressed = false;
        InUse = false;
    }

    public virtual void Pressed()
    {
        IsPressed = true;

        var charge = AbilityController.Instance.GetAbility(Type.CHARGE) as AbilityCharge;
        var has_charge = AbilityController.Instance.HasModifier(Info.type, Type.CHARGE);
        if (Info.type == Type.CHARGE)
        {
            // Do nothing
        }
        else if (has_charge && charge != null)
        {
            charge.BeginCharge(GetBaseCooldown() * Player.Instance.GlobalCooldownMultiplier * 0.5f);
        }
        else
        {
            Trigger();
        }
    }

    public virtual void Released()
    {
        IsPressed = false;

        var charge = AbilityController.Instance.GetAbility(Type.CHARGE) as AbilityCharge;
        var has_charge = AbilityController.Instance.HasModifier(Info.type, Type.CHARGE);
        if (Info.type == Type.CHARGE)
        {
            // Do nothing
        }
        else if (has_charge && charge != null)
        {
            if (charge.EndCharge())
            {
                Trigger();
            }
            else
            {
                // Do nothing
            }
        }
    }

    public void TryRelease()
    {
        if (!IsPressed) return;
        if (cr_release != null) return;
        cr_release = StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return WaitForGamestatePlaying();
            Released();
            cr_release = null;
        }
    }

    public virtual void Trigger()
    {
        // Trigger ability
        onTrigger?.Invoke();
    }

    private IEnumerator WaitForGamestatePlaying()
    {
        while(GameStateController.Instance.GameState != GameStateType.PLAYING)
        {
            yield return null;
        }
    }
    #endregion
    #region COOLDOWN
    public abstract float GetBaseCooldown();
    public void StartCooldown() => StartCooldown(GetBaseCooldown());

    public void StartCooldown(float duration)
    {
        InUse = false || CanPressWhileOnCooldown();

        var mul_charge = HasModifier(Type.CHARGE) ? 0 : 1;
        var mul_global = Player.Instance.GlobalCooldownMultiplier;

        TimeCooldownStart = Time.time;
        TimeCooldownEnd = TimeCooldownStart + duration * mul_global * mul_charge;
        StartCoroutine(WaitForCooldownCr());

        IEnumerator WaitForCooldownCr()
        {
            while(Time.time < TimeCooldownEnd)
            {
                yield return null;
            }

            onCooldownComplete?.Invoke();
        }
    }

    public void AdjustCooldownFlat(float seconds)
    {
        TimeCooldownEnd += seconds;
    }

    public virtual bool CanPressWhileOnCooldown() => false;
    #endregion
}
