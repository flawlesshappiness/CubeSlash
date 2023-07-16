using System.Collections;
using UnityEngine;

public abstract class Ability : MonoBehaviourExtended
{
    [Header("ABILITY")]
    public AbilityInfo Info;

    public event System.Action onTrigger;
    public event System.Action onCooldownComplete;

    public enum Type { DASH, SPLIT, EXPLODE, CHAIN, MINES }
    public Player Player { get { return Player.Instance; } }
    public bool IsPressed { get; set; }
    public float TimeCooldownStart { get; private set; }
    public float TimeCooldownEnd { get; protected set; }
    public float TimeCooldownLeft { get { return IsOnCooldown ? TimeCooldownEnd - Time.time : 0f; } }
    public float CooldownPercentage { get { return (Time.time - TimeCooldownStart) / (TimeCooldownEnd - TimeCooldownStart); } }
    public bool IsOnCooldown { get { return Time.time < TimeCooldownEnd; } }
    public bool InUse { get; set; }
    public BodypartAbility Bodypart { get; set; }

    private Coroutine cr_release;

    protected GameAttribute att_cooldown_multiplier;

    private void OnEnable()
    {
        GameController.Instance.onResume += OnResume;
    }

    private void OnDisable()
    {
        GameController.Instance.onResume -= OnResume;
    }

    public virtual void InitializeFirstTime()
    {
        att_cooldown_multiplier = GameAttributeController.Instance.GetAttribute(GameAttributeType.player_global_cooldown_multiplier);
    }

    protected virtual void OnResume()
    {

    }

    protected virtual void Update()
    {
        UpdateCooldown();
    }

    public bool IsModifier() => AbilityController.Instance.HasModifier(Info.type);
    public bool IsEquipped() => AbilityController.Instance.IsAbilityEquipped(Info.type);

    public void SetBodypart(BodypartAbility bdp)
    {
        Bodypart = bdp;
    }

    #region INPUT
    public void ResetInput()
    {
        IsPressed = false;
        InUse = false;
    }

    public virtual void Pressed()
    {
        IsPressed = true;
        Trigger();
    }

    public virtual void Released()
    {
        IsPressed = false;
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
        while (GameStateController.Instance.GameState != GameStateType.PLAYING)
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

        var mul_global = att_cooldown_multiplier.ModifiedValue.float_value;

        TimeCooldownStart = Time.time;
        TimeCooldownEnd = TimeCooldownStart + duration * mul_global;
        StartCoroutine(WaitForCooldownCr());

        IEnumerator WaitForCooldownCr()
        {
            while (Time.time < TimeCooldownEnd)
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

    private void UpdateCooldown()
    {
        var end = TimeCooldownEnd - TimeCooldownStart;
        var t = end == 0 ? 1 : (Time.time - TimeCooldownStart) / end;
        if (Bodypart != null)
        {
            Bodypart.SetCooldown(1f - Mathf.Clamp01(t));
        }
    }
    #endregion
}
