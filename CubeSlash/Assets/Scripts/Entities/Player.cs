using Flawliz.Console;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviourExtended
{
    public static Player Instance;
    public PlayerSettings settings;
    public Character Character { get { return GetComponentOnce<Character>(ComponentSearchType.CHILDREN); } }
    public Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.CHILDREN); } }
    public MinMaxInt Experience { get; private set; } = new MinMaxInt();
    public MinMaxInt Health { get; private set; } = new MinMaxInt();
    public int Level { get; private set; }
    public bool HasLevelledUp { get; private set; }
    public bool IsDead { get; private set; }
    public int AbilityPoints { get; set; }
    public MultiLock InputLock { get; set; } = new MultiLock();
    public MultiLock InvincibilityLock { get; set; } = new MultiLock();
    public MultiLock AbilityLock { get; set; } = new MultiLock();
    public MultiLock MovementLock { get; set; } = new MultiLock();
    public MultiLock DragLock { get; set; } = new MultiLock();
    public Vector3 MoveDirection { get; set; }
    public float DistanceCollect { get; private set; } = 3f;
    public float SpeedMove { get; private set; } = 5;

    public System.Action onLevelUp;
    public System.Action onDeath;

    public Ability AbilityQueued { get; private set; }

    public void Initialize()
    {
        // Health
        Health.Min = 0;
        Health.Max = 6;
        Health.Value = Health.Max;
        Health.onMin += OnDeath;

        // Experience
        Experience.Min = 0;
        Experience.Max = settings.experience_min;
        Experience.Value = Experience.Min;
        Experience.onMax += OnLevelUp;

        // Character
        Character.Initialize();
        MoveDirection = transform.up;
    }

    private void OnEnable()
    {
        PlayerInput.OnAbilityButtonDown += PressAbility;
        PlayerInput.OnAbilityButtonUp += ReleaseAbility;
    }

    private void OnDisable()
    {
        PlayerInput.OnAbilityButtonDown -= PressAbility;
        PlayerInput.OnAbilityButtonUp -= ReleaseAbility;
    }

    private void Update()
    {
        MoveUpdate();
        QueuedAbilityUpdate();
    }
    #region ABILITIES
    public void AttachAbility(Ability ability)
    {
        ability.transform.parent = Character.transform;
        ability.transform.position = transform.position;
        ability.transform.rotation = transform.rotation;
        ability.Player = this;
    }

    public void ReapplyAbilities()
    {
        foreach (var ability in AbilityController.Instance.GetEquippedAbilities())
        {
            ability.ApplyActive();
        }
    }

    private bool CanUseAbility(Ability ability)
    {
        var not_paused = !GameController.Instance.IsPaused;
        var not_blocking = AbilityLock.IsFree;
        var not_cooldown = !ability.OnCooldown;
        return not_blocking && not_cooldown && not_paused;
    }

    private void PressAbility(PlayerInput.ButtonType button)
    {
        if (InputLock.IsLocked) return;
        var ability = AbilityController.Instance.GetEquippedAbility(button);
        if (ability == null) return;

        if (CanUseAbility(ability))
        {
            ability.Pressed();
            AbilityQueued = null;
        }
        else if (ability.TimeCooldownLeft < 0.5f)
        {
            AbilityQueued = ability;
        }
    }

    private void ReleaseAbility(PlayerInput.ButtonType button)
    {
        if (InputLock.IsLocked) return;
        var ability = AbilityController.Instance.GetEquippedAbility(button);
        if (ability == null) return;

        if (ability.IsPressed)
        {
            ability.Released();
        }

        if (AbilityQueued == ability)
        {
            AbilityQueued = null;
        }
    }

    private void QueuedAbilityUpdate()
    {
        if (AbilityQueued)
        {
            if (CanUseAbility(AbilityQueued))
            {
                AbilityQueued.Pressed();
                AbilityQueued = null;
            }
        }
    }
    #endregion
    #region MOVE
    private void MoveUpdate()
    {
        var dir = PlayerInput.MoveDirection;
        if (MovementLock.IsFree && InputLock.IsFree && dir.magnitude > 0.5f)
        {
            MoveDirection = dir.normalized;
            Move(dir.normalized);
        }
        else if (DragLock.IsFree)
        {
            // Decelerate
            Rigidbody.velocity = Rigidbody.velocity * 0.7f;
        }
    }

    private void Move(Vector3 direction)
    {
        Rigidbody.velocity = direction * SpeedMove;
        Character.SetLookDirection(direction);
    }
    #endregion
    #region ENEMY
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var enemy = collision.gameObject.GetComponentInParent<Enemy>();
        if (enemy == null) return;
        Damage(1, enemy.transform.position);
    }
    #endregion
    #region HEALTH
    private void OnDeath()
    {
        if (!IsDead)
        {
            Kill();
        }
    }

    public void Kill()
    {
        IsDead = true;

        InstantiateParticle("Particles/ps_burst")
            .Position(transform.position)
            .Destroy(1)
            .Play();

        InstantiateParticle("Particles/ps_flash")
            .Position(transform.position)
            .Scale(Character.transform.localScale * 5)
            .Destroy(1)
            .Play();

        gameObject.SetActive(false);
        onDeath?.Invoke();
    }

    public void Damage(int amount, Vector3 damage_origin)
    {
        if (InvincibilityLock.IsFree)
        {
            if (!GameController.DAMAGE_DISABLED)
            {
                Health.Value -= amount.Abs();
            }

            if (Health.Value > 0)
            {
                StartCoroutine(PlayerDamageInvincibilityCr(2));
                StartCoroutine(PlayerDamageFlashCr(2));

                var dir = transform.position - damage_origin;
                StartCoroutine(PlayerDamagePushCr(dir, 0.15f));
            }
        }
    }

    private IEnumerator PlayerDamageInvincibilityCr(float time)
    {
        InvincibilityLock.AddLock("Damage");
        yield return new WaitForSeconds(time);
        InvincibilityLock.RemoveLock("Damage");

        // Check if still inside an enemy
        var hits = Physics2D.OverlapCircleAll(transform.position, Character.Trigger.radius);
        foreach (var hit in hits)
        {
            var e = hit.GetComponentInParent<Enemy>();
            if (e)
            {
                Damage(1, e.transform.position);
                break;
            }
        }
    }

    private IEnumerator PlayerDamageFlashCr(float time)
    {
        var time_end = Time.time + time;
        while (Time.time < time_end)
        {
            Character.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            Character.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
        Character.gameObject.SetActive(true);
    }

    private IEnumerator PlayerDamagePushCr(Vector3 dir, float time)
    {
        MovementLock.AddLock("Damage");
        DragLock.AddLock("Damage");

        Rigidbody.velocity = dir.normalized * 50f;
        var time_end = Time.time + time;
        while (Time.time < time_end)
        {
            Rigidbody.velocity = Rigidbody.velocity * 0.99f;
            yield return null;
        }

        MovementLock.RemoveLock("Damage");
        DragLock.RemoveLock("Damage");
    }
    #endregion
    #region EXPERIENCE
    private void OnLevelUp()
    {
        if (!HasLevelledUp)
        {
            HasLevelledUp = true;
            Level++;
            AbilityPoints++;
            onLevelUp?.Invoke();
        }
    }

    public void ResetExperience()
    {
        var t_level = Level / (float)settings.experience_level_max;
        var t_exp = settings.curve_experience.Evaluate(t_level);
        Experience.Max = (int)(Mathf.Lerp(settings.experience_min, settings.experience_max, t_exp));
        Experience.Value = Experience.Min;
        HasLevelledUp = false;
    }
    #endregion
}
