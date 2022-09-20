using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : Character
{
    public static Player Instance;
    [SerializeField] private PlayerSettings settings;
    public MinMaxFloat Experience { get; private set; } = new MinMaxFloat();
    public Health Health { get; private set; } = new Health();
    public int Level { get; private set; }
    public bool HasLevelledUp { get; private set; }
    public bool IsDead { get; private set; }
    public MultiLock InputLock { get; set; } = new MultiLock();
    public MultiLock InvincibilityLock { get; set; } = new MultiLock();
    public MultiLock AbilityLock { get; set; } = new MultiLock();
    public Vector3 MoveDirection { get; set; }
    public Ability AbilityQueued { get; private set; }

    // UPGRADE VALUES
    public float ChanceToAvoidDamage { get; private set; }
    public float GlobalCooldownMultiplier { get; private set; }
    public float ExperienceMultiplier { get; private set; }
    public float DistanceCollect { get; private set; }
    public float SpeedBoostPerExp { get; private set; }

    public event System.Action onLevelUp;
    public event System.Action onDeath;

    private ParticleSystem ps_move;

    public void Initialize()
    {
        // Experience
        Experience.onMax += OnLevelUp;

        // Speed
        Rigidbody.mass = settings.mass;

        // Character
        SetBody(settings.body);
        Body.transform.localEulerAngles = Vector3.one * settings.size;
        MoveDirection = transform.up;

        // Particles
        ps_move = Instantiate(Resources.Load<ParticleSystem>("Particles/ps_player_move"));
        ps_move.transform.parent = Body.transform;
        ps_move.transform.position = Body.transform.position;
        ps_move.ModifyEmission(e => e.enabled = false);
    }

    public void ResetValues()
    {
        Level = 0;
        IsDead = false;
        ResetExperience();
        ResetHealth();
        ResetUpgradeValues();
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

    protected override void OnUpdate()
    {
        base.OnUpdate();
        QueuedAbilityUpdate();
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        MoveUpdate();
    }

    private void MoveUpdate()
    {
        var dir = PlayerInput.MoveDirection;
        if (InputLock.IsFree)
        {
            if (dir.magnitude > 0.5f)
            {
                MoveDirection = dir.normalized;
                Move(MoveDirection);
                Body.SetLookDirection(MoveDirection);
                ps_move.ModifyEmission(e => e.enabled = true);
            }
            else
            {
                MoveToStop();
                ps_move.ModifyEmission(e => e.enabled = false);
            }
        }
    }

    #region ABILITIES
    public void AttachAbility(Ability ability)
    {
        ability.transform.parent = transform;
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
    #region UPGRADES
    public void OnUpgradeSelected(Upgrade upgrade)
    {
        /*
        if(upgrade.data.type == UpgradeData.Type.PLAYER_HEALTH)
        {
            if(upgrade.level == 1)
            {
                Health.AddHealth(HealthPoint.Type.TEMPORARY);
            }
            else if(upgrade.level == 2)
            {
                Health.AddHealth(HealthPoint.Type.FULL);
            }
        }
        */
    }

    public void ReapplyUpgrades()
    {
        ResetUpgradeValues();

        /*
        UpgradeController.Instance.Database.upgrades.Select(data => UpgradeController.Instance.GetUpgrade(data.type))
            .ToList().ForEach(upgrade => ApplyUpgrade(upgrade));
        */
    }

    private void ResetUpgradeValues()
    {
        LinearAcceleration = settings.linear_acceleration;
        LinearVelocity = settings.linear_velocity;
        ChanceToAvoidDamage = 0;
        GlobalCooldownMultiplier = 1f;
        DistanceCollect = 3f;
        ExperienceMultiplier = 1f;
    }

    private void ApplyUpgrade(Upgrade upgrade)
    {
        /*
        if(upgrade.data.type == UpgradeData.Type.PLAYER_HEALTH)
        {
            if(upgrade.level >= 3)
            {
                ChanceToAvoidDamage += 0.2f;
            }
        }
        else if(upgrade.data.type == UpgradeData.Type.PLAYER_EXP)
        {
            if(upgrade.level >= 1)
            {
                DistanceCollect += 2f;
            }

            if (upgrade.level >= 2)
            {
                DistanceCollect += 2f;
                ExperienceMultiplier += 0.2f;
            }

            if (upgrade.level >= 3)
            {
                // Undecided
            }
        }
        else if(upgrade.data.type == UpgradeData.Type.PLAYER_SPEED)
        {
            if(upgrade.level >= 1)
            {
                LinearVelocity += 2f;
            }

            if(upgrade.level >= 2)
            {
                LinearVelocity += 2f;
                LinearAcceleration += 7f;
            }

            if(upgrade.level >= 3)
            {
                LinearAcceleration += 5f;
                GlobalCooldownMultiplier -= 0.2f;
            }
        }
        */
    }
    #endregion
    #region ENEMY
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var enemy = collision.gameObject.GetComponentInParent<Enemy>();
        if (enemy == null) return;
        Damage(enemy.transform.position);
    }

    public void PushEnemiesInArea(Vector3 position, float radius, float force, AnimationCurve curve_force = null)
    {
        var hits = new List<Enemy>();
        foreach (var hit in Physics2D.OverlapCircleAll(position, radius))
        {
            var enemy = hit.GetComponentInParent<Enemy>();
            if (enemy != null && !hits.Contains(enemy))
            {
                hits.Add(enemy);
                var dir = enemy.transform.position - position;
                var dist = Vector3.Distance(enemy.transform.position, position);
                var t_dist = dist / radius;

                var t_force = curve_force == null ? 1 : curve_force.Evaluate(t_dist);
                var calc_force = force * t_force;
                var dir_knock = dir.normalized.ToVector2() * calc_force;
                enemy.Knockback(dir_knock, false, true);
            }
        }
    }
    #endregion
    #region HEALTH
    private void ResetHealth()
    {
        if (Health == null) Health = new Health();
        Health.Clear();
        for (int i = 0; i < 5; i++) Health.AddHealth(HealthPoint.Type.FULL);
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
            .Scale(Body.transform.localScale * 5)
            .Destroy(1)
            .Play();

        gameObject.SetActive(false);
        onDeath?.Invoke();
    }

    public void Damage(Vector3 damage_origin)
    {
        if (IsDead) return;
        if (InvincibilityLock.IsLocked) return;

        if (!GameController.DAMAGE_DISABLED)
        {
            if(ChanceToAvoidDamage == 0 || Random.Range(0f, 1f) < ChanceToAvoidDamage)
            {
                Health.Damage();
            }
            else
            {
                // Avoided damage
            }
        }

        if (Health.IsDead())
        {
            Kill();
        }
        else
        {
            StartCoroutine(PlayerDamageInvincibilityCr(2));
            StartCoroutine(PlayerDamageFlashCr(2));

            var dir = transform.position - damage_origin;
            StartCoroutine(PlayerDamagePushCr(dir, 0.15f));
        }
    }

    private IEnumerator PlayerDamageInvincibilityCr(float time)
    {
        InvincibilityLock.AddLock("Damage");
        yield return new WaitForSeconds(time);
        InvincibilityLock.RemoveLock("Damage");

        // Check if still inside an enemy
        var hits = Physics2D.OverlapCircleAll(transform.position, Body.Trigger.radius);
        foreach (var hit in hits)
        {
            var e = hit.GetComponentInParent<Enemy>();
            if (e)
            {
                Damage(e.transform.position);
                break;
            }
        }
    }

    private IEnumerator PlayerDamageFlashCr(float time)
    {
        var time_end = Time.time + time;
        while (Time.time < time_end)
        {
            Body.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            Body.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
        Body.gameObject.SetActive(true);
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
            var ps = InstantiateParticle("Particles/ps_level_up")
                .Parent(transform)
                .Position(transform.position)
                .Play()
                .Destroy(5);

            StartCoroutine(PushCr(0.4f));

            HasLevelledUp = true;
            Level++;
            onLevelUp?.Invoke();
        }

        IEnumerator PushCr(float delay)
        {
            yield return new WaitForSeconds(delay);
            PushEnemiesInArea(transform.position, 12, 500);
        }
    }

    public void AddExperience()
    {
        Experience.Value += 1f * ExperienceMultiplier;
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
