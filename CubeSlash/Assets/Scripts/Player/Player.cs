using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Player : Character
{
    public static Player Instance;
    [SerializeField] private PlayerSettings settings;
    [SerializeField] private StatCollection stats;
    [SerializeField] private GameObject g_invincible;
    [SerializeField] private ParticleSystem ps_collect_meat, ps_collect_plant, ps_collect_health, ps_level_up;
    [SerializeField] private FMODEventReference event_ability_on_cooldown;
    [SerializeField] private FMODEventReference event_levelup_slide;
    [SerializeField] private FMODEventReference event_levelup;
    [SerializeField] private FMODEventReference event_damage;
    [SerializeField] private FMODEventReference sfx_collect_experience;
    [SerializeField] private FMODEventReference sfx_collect_health;
    [SerializeField] private FMODEventReference sfx_level_up;
    public PlayerBody PlayerBody { get { return Body as PlayerBody; } }
    public MinMaxFloat Experience { get; private set; } = new MinMaxFloat();
    public Health Health { get; private set; } = new Health();
    public int Level { get; private set; }
    public int LevelsUntilAbility { get; private set; }
    public bool HasLevelledUp { get; private set; }
    public bool IsDead { get; private set; }
    public MultiLock InputLock { get; set; } = new MultiLock();
    public MultiLock InvincibilityLock { get; set; } = new MultiLock();
    public MultiLock AbilityLock { get; set; } = new MultiLock();
    public Vector3 MoveDirection { get; set; }
    public Ability AbilityQueued { get; private set; }
    public StatValueCollection Values { get; private set; }

    // UPGRADE VALUES
    public float ChanceToAvoidDamage { get; private set; }
    public float GlobalCooldownMultiplier { get; private set; }
    public float CollectCooldownReduction { get; private set; }
    public float ExperienceMultiplier { get; private set; }
    public float CollectRadius { get; private set; }
    public bool CollectSpeedBoost { get; private set; }
    public bool ConvertHealthToArmor { get; private set; }
    public bool InfiniteDrag { get; private set; }
    public bool KillEnemyShieldRegen { get; private set; }
    public bool PlantExpHealthRegen { get; private set; }

    public event System.Action onLevelUp;
    public event System.Action onDeath;
    public event System.Action<Collider2D> onTriggerEnter;

    private float timestamp_collect_last;
    private int enemy_kills_until_shield_regen;
    private int plant_exp_until_health_regen;

    public void Initialize()
    {
        // Values
        Values = new StatValueCollection(stats);

        // Experience
        Experience.onMax += OnLevelUp;

        // Setup
        MoveDirection = transform.up;
        g_invincible.SetActive(false);
    }

    public void Clear()
    {
        AbilityController.Instance.Clear();
        UpgradeController.Instance.ClearUpgrades();
    }

    public void SetPlayerBody(PlayerBodySettings settings)
    {
        SetBody(settings.body);
        PlayerBody.Settings = settings;
        Body.Size = settings.body_size;
        Rigidbody.mass = settings.mass;

        Body.transform.localEulerAngles = Vector3.one * settings.body_size;
        MoveDirection = transform.up;

        // Add ability
        var ability = AbilityController.Instance.UnlockAbility(settings.ability_type);
        AbilityController.Instance.EquipAbility(ability, PlayerInput.ButtonType.WEST);

        ResetValues();
        ReapplyUpgrades();
        ReapplyAbilities();
    }

    public void ResetValues()
    {
        Level = 0;
        IsDead = false;
        ResetHealth();
        ResetExperience();
        ResetLevelsUntilAbility();
        ResetKillsUntilShieldRegen();
        ResetPlantExperienceUntilHealthRegen();
        ResetLocks();
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

    private void ResetLocks()
    {
        AbilityLock.ClearLock();
    }

    private void MoveUpdate()
    {
        if (!GameController.Instance.IsGameStarted) return;

        // Update move values
        var flat_velocity = Values.GetFloatValue("FlatVelocity");
        var flat_acceleration = Values.GetFloatValue("FlatAcceleration");
        var perc_velocity = Values.GetFloatValue("PercVelocity");
        var perc_acceleration = Values.GetFloatValue("PercAcceleration");

        var t_collect_boost = (Time.time - timestamp_collect_last) / 0.5f;
        var collect_boost_acceleration = CollectSpeedBoost ? Mathf.Lerp(10, 0, t_collect_boost) : 0;
        var collect_boost_velocity = CollectSpeedBoost ? Mathf.Lerp(8, 0, t_collect_boost) : 0;

        LinearAcceleration = (PlayerBody.Settings.linear_acceleration + flat_acceleration + collect_boost_acceleration) * perc_acceleration;
        LinearVelocity = (PlayerBody.Settings.linear_velocity + flat_velocity + collect_boost_velocity) * perc_velocity;
        LinearDrag = PlayerBody.Settings.linear_drag;

        // Move
        var dir = PlayerInput.MoveDirection;
        if (InputLock.IsFree)
        {
            if (dir.magnitude > 0.5f)
            {
                MoveDirection = dir.normalized;
                Move(MoveDirection);
                Body.SetLookDirection(MoveDirection);
            }
            else if(InfiniteDrag && !IsStunned())
            {
                Rigidbody.velocity = Vector2.zero;
            }
            else
            {
                MoveToStop();
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
        var not_cooldown = !ability.IsOnCooldown;
        var game_started = GameController.Instance.IsGameStarted;
        return not_blocking && not_cooldown && not_paused && game_started;
    }

    private void PressAbility(PlayerInput.ButtonType button)
    {
        if (InputLock.IsLocked) return;
        var ability = AbilityController.Instance.GetEquippedAbility(button);
        if (ability == null) return;
        if (GameStateController.Instance.GameState != GameStateType.PLAYING) return;

        if (CanUseAbility(ability))
        {
            ability.Pressed();
            AbilityQueued = null;
        }
        else
        {
            event_ability_on_cooldown.Play();
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
            ability.TryRelease();
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
    public void ReapplyUpgrades()
    {
        Values.ResetValues();
        ApplyUpgrades();
        ApplyUpgradeValues();
        ApplyBodyparts();
    }

    private void ApplyUpgrades()
    {
        UpgradeController.Instance.GetUnlockedUpgrades()
            .Where(info => !info.require_ability)
            .ToList().ForEach(info => Values.ApplyEffects(info.upgrade.effects));
    }

    private void ApplyUpgradeValues()
    {
        ChanceToAvoidDamage = Values.GetFloatValue("AvoidDamage");
        GlobalCooldownMultiplier = 1f - Values.GetFloatValue("CooldownReduc");
        CollectRadius = Values.GetFloatValue("CollectRadius");
        ExperienceMultiplier = 1f + Values.GetFloatValue("ExpBonus");
        CollectCooldownReduction = Values.GetFloatValue("CollectCooldownReduc");
        CollectSpeedBoost = Values.GetBoolValue("CollectSpeedBoost");
        ConvertHealthToArmor = Values.GetBoolValue("ConvertHealthToArmor");
        Body.Size = PlayerBody.Settings.body_size + Values.GetFloatValue("BodySize");
        InfiniteDrag = Values.GetBoolValue("InfiniteDrag");
        KillEnemyShieldRegen = Values.GetBoolValue("KillEnemyShieldRegen");
        PlantExpHealthRegen = Values.GetBoolValue("PlantExpHealthRegen");
    }

    public void OnUpgradeSelected(Upgrade upgrade)
    {
        foreach(var e in upgrade.effects)
        {
            if(e.variable.name == "Health")
            {
                for (int i = 0; i < e.variable.value_int; i++)
                {
                    Health.AddHealth(HealthPoint.Type.FULL);
                }
            }
            else if (e.variable.name == "Armor")
            {
                for (int i = 0; i < e.variable.value_int; i++)
                {
                    Health.AddHealth(HealthPoint.Type.TEMPORARY);
                }
            }
            else if(e.variable.name == "ConvertHealthToArmor")
            {
                Health.SetConvertHealthToArmorEnabled(true);
            }
        }
    }

    private void ApplyBodyparts()
    {
        PlayerBody.ClearBodyparts();

        foreach(var ability in AbilityController.Instance.GetEquippedAbilities())
        {
            if (ability.prefab_bodypart == null) continue;

            var bps = PlayerBody.CreateBodyparts(ability.prefab_bodypart);

            foreach(var bp in bps)
            {
                bp.Initialize(ability);
            }
        }
    }
    #endregion
    #region ENEMY
    private void OnTriggerEnter2D(Collider2D collision)
    {
        onTriggerEnter?.Invoke(collision);

        var hurt = collision.gameObject.GetComponentInParent<IHurt>();
        if (hurt == null) return;
        Damage(hurt.GetPosition());
    }

    public static void PushEnemiesInArea(Vector3 position, float radius, float force, AnimationCurve curve_force = null)
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
                enemy.Knockback(dir_knock, false, false);
            }
        }
    }

    public void KillEnemy(IKillable k)
    {
        k.Kill();
        DecrementKillsUntilShieldRegen();
    }

    private void DecrementKillsUntilShieldRegen()
    {
        if (!KillEnemyShieldRegen) return;

        enemy_kills_until_shield_regen--;
        if(enemy_kills_until_shield_regen <= 0)
        {
            ResetKillsUntilShieldRegen();
            Health.AddHealth(HealthPoint.Type.TEMPORARY);
        }
    }

    private void ResetKillsUntilShieldRegen()
    {
        enemy_kills_until_shield_regen += 100;
    }
    #endregion
    #region HEALTH
    public void Suicide()
    {
        if (IsDead) return;
        Kill();
    }

    private void ResetHealth()
    {
        if (Health == null) Health = new Health();
        Health.Clear();

        var init_health = PlayerBody.Settings.health + stats.GetStat("Health").value_int;
        for (int i = 0; i < init_health; i++) Health.AddHealth(HealthPoint.Type.FULL);

        var init_temp = PlayerBody.Settings.armor + stats.GetStat("Armor").value_int;
        for (int i = 0; i < init_temp; i++) Health.AddHealth(HealthPoint.Type.TEMPORARY);
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
            if(ChanceToAvoidDamage == 0 || Random.Range(0f, 1f) > ChanceToAvoidDamage)
            {
                Health.Damage();
                event_damage.Play();
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

            ResetStun();
            var dir = transform.position - damage_origin;
            StartCoroutine(PlayerDamagePushCr(dir, 0.15f));
        }
    }

    public void CollectHealth(HealthPoint.Type type)
    {
        // Heal
        if(type == HealthPoint.Type.TEMPORARY)
        {
            Health.AddHealth(HealthPoint.Type.TEMPORARY);
        }
        else if(type == HealthPoint.Type.FULL)
        {
            Health.Heal();
        }

        // Effect
        ps_collect_health.Play();

        // Sound
        FMODController.Instance.PlayWithLimitDelay(sfx_collect_health);
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
            //Body.gameObject.SetActive(false);
            g_invincible.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            //Body.gameObject.SetActive(true);
            g_invincible.SetActive(false);
            yield return new WaitForSeconds(0.1f);
        }
        //Body.gameObject.SetActive(true);
        g_invincible.SetActive(false);
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
        LevelUp();
    }

    private void LevelUp()
    {
        if (!HasLevelledUp)
        {
            HasLevelledUp = true;
            Level++;
            LevelsUntilAbility--;
            onLevelUp?.Invoke();
        }

        IEnumerator PushCr(float delay)
        {
            event_levelup_slide.Play();
            yield return new WaitForSeconds(delay);
            event_levelup_slide.Stop();
            event_levelup.Play();
            PushEnemiesInArea(transform.position, 12, 500);
        }
    }

    public void CollectExperience(ExperienceType type)
    {
        timestamp_collect_last = Time.time;

        var mul_difficulty = GameSettings.Instance.experience_mul_difficulty.Evaluate(DifficultyController.Instance.DifficultyValue);
        Experience.Value += 1f * ExperienceMultiplier * mul_difficulty;

        // Adjust ability cooldown
        AbilityController.Instance.GetEquippedAbilities()
            .ForEach(a => a.AdjustCooldownFlat(CollectCooldownReduction));

        if(type == ExperienceType.PLANT)
        {
            DecrementPlantExperienceUntilHealthRegen();
            ps_collect_plant.Play();
        }
        else if(type == ExperienceType.MEAT)
        {
            ps_collect_meat.Play();
        }

        // Sound
        FMODController.Instance.PlayWithLimitDelay(sfx_collect_experience);
    }

    private void DecrementPlantExperienceUntilHealthRegen()
    {
        if (!PlantExpHealthRegen) return;
        plant_exp_until_health_regen--;

        if (plant_exp_until_health_regen <= 0)
        {
            Health.Heal();
            ResetPlantExperienceUntilHealthRegen();
        }
    }

    private void ResetPlantExperienceUntilHealthRegen()
    {
        plant_exp_until_health_regen += 50;
    }

    public void ResetExperience()
    {
        var max = (int)settings.curve_experience.Evaluate(Level);
        Experience.Max = max;
        Experience.Value = Experience.Min;
        HasLevelledUp = false;
    }

    public bool CanGainAbility() => LevelsUntilAbility <= 0;

    public void ResetLevelsUntilAbility()
    {
        LevelsUntilAbility = GetMaxLevelsUntilAbility();
    }

    public void CheatLevelsUntilNextAbility(int i)
    {
        LevelsUntilAbility = i;
    }

    public int GetMaxLevelsUntilAbility()
    {
        var count = AbilityController.Instance.GetUnlockedAbilities().Count;
        if (count < 2)
        {
            return 5;
        }
        else if (count < 3)
        {
            return 8;
        }
        else
        {
            return 10;
        }
    }
    #endregion
    #region EFFECTS
    public void PlayLevelUpFX()
    {
        ps_level_up.Play();
        sfx_level_up.Play();
    }
    #endregion
}
