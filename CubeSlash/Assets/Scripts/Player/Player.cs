using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : Character
{
    public static Player Instance;
    [SerializeField] private PlayerSettings settings;
    [SerializeField] private GameObject g_invincible;
    [SerializeField] private ParticleSystem ps_collect_meat, ps_collect_plant, ps_collect_health, ps_level_up, ps_upgrade;
    [SerializeField] private ParticleSystem ps_ability_off_cooldown, ps_avoid_damage;
    public PlayerBody PlayerBody { get { return Body as PlayerBody; } }
    public MinMaxFloat Experience { get; private set; } = new MinMaxFloat();
    public PlayerInfo Info { get { return PlayerInfo.Instance; } }
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

    // UPGRADE VALUES
    public float ChanceToAvoidDamage { get; private set; }
    public float GlobalCooldownMultiplier { get; private set; }
    public float CollectCooldownReduction { get; private set; }
    public float ExperienceMultiplier { get; private set; }
    public float CollectRadius { get; private set; }
    public bool CollectSpeedBoost { get; private set; }
    public bool ConvertHealthToArmor { get; private set; }
    public bool InfiniteDrag { get; private set; }
    public int KillEnemyShieldRegen { get; private set; }
    public int PlantExpHealthRegen { get; private set; }

    private const float COLLECT_RADIUS = 3;

    public event System.Action onLevelUp;
    public event System.Action onDeath;
    public event System.Action<Collider2D> onTriggerEnter;
    public event System.Action onValuesUpdated;

    private float timestamp_collect_last;
    private int enemy_kills_until_shield_regen;
    private int plant_exp_until_health_regen;

    public void Initialize()
    {
        // Experience
        Experience.onMax += OnLevelUp;

        // Upgrades
        UpgradeController.Instance.onUpgradeUnlocked += OnUpgradeUnlocked;
        PlayerValueController.Instance.onValuesUpdated += OnValuesUpdated;

        // Setup
        MoveDirection = transform.up;
        g_invincible.SetActive(false);

        Clear();
    }

    public void Clear()
    {
        AbilityController.Instance.Clear();
        UpgradeController.Instance.ClearUpgrades();

        ResetPlayerBody();
        SetPrimaryAbility(Save.PlayerBody.primary_ability);
        UpdateBodyparts();

        gameObject.SetActive(true);
        transform.rotation = Quaternion.identity;
        Body.SetLookDirection(transform.up);
    }

    public void ResetPlayerBody()
    {
        var db_body = Database.Load<PlayerBodyDatabase>();
        var info = db_body.collection.First(info => info.type == Save.PlayerBody.body_type);
        SetPlayerBody(info);

        var skin = info.skins[Save.PlayerBody.body_skin];
        PlayerBody.SetBodySprite(skin);
    }

    public void SetPlayerBody(PlayerBodyInfo info)
    {
        SetBody(info.prefab);
        Body.Size = Info.body_size;
        Rigidbody.mass = Info.mass;

        Body.transform.localScale = Vector3.one * Info.body_size;
        MoveDirection = transform.up;
    }

    public void SetPlayerBody(PlayerBodySettings settings)
    {
        SetBody(settings.body);
        Body.Size = settings.body_size;
        Rigidbody.mass = settings.mass;

        Body.transform.localEulerAngles = Vector3.one * settings.body_size;
        MoveDirection = transform.up;

        g_invincible.SetActive(false);

        // Ability
        var ability = AbilityController.Instance.GainAbility(settings.ability_type);
        AbilityController.Instance.EquipAbility(ability, PlayerInput.ButtonType.WEST);

        ResetValues();
        UpdateUpgradeValues();
        UpdateBodyparts();
    }

    public void SetPrimaryAbility(Ability.Type type)
    {
        AbilityController.Instance.Clear();

        Save.PlayerBody.primary_ability = type;
        AbilityController.Instance.GainAbility(type);
        var ability = AbilityController.Instance.GetAbility(type);
        AbilityController.Instance.EquipAbility(ability, PlayerInput.ButtonType.WEST);
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
        InvincibilityLock.ClearLock();
        DragLock.ClearLock();
    }

    private void MoveUpdate()
    {
        if (!GameController.Instance.IsGameStarted) return;

        // Update move values
        var flat_velocity = PlayerValueController.Instance.GetFloatValue(StatID.player_velocity_flat);
        var flat_acceleration = PlayerValueController.Instance.GetFloatValue(StatID.player_acceleration_flat);
        var perc_velocity = PlayerValueController.Instance.GetFloatValue(StatID.player_velocity_perc);
        var perc_acceleration = PlayerValueController.Instance.GetFloatValue(StatID.player_acceleration_perc);

        var t_collect_boost = (Time.time - timestamp_collect_last) / 0.5f;
        var collect_boost_acceleration = CollectSpeedBoost ? Mathf.Lerp(10, 0, t_collect_boost) : 0;
        var collect_boost_velocity = CollectSpeedBoost ? Mathf.Lerp(8, 0, t_collect_boost) : 0;

        LinearAcceleration = (Info.linear_acceleration + flat_acceleration + collect_boost_acceleration) * perc_acceleration;
        LinearVelocity = (Info.linear_velocity + flat_velocity + collect_boost_velocity) * perc_velocity;
        LinearDrag = Info.linear_drag;

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

    public void SetRigidbodyEnabled(bool enabled)
    {
        Rigidbody.isKinematic = !enabled;

        if (!enabled)
        {
            Rigidbody.velocity = Vector3.zero;
        }
    }

    #region ABILITIES
    private bool CanPressAbility(Ability ability)
    {
        var not_paused = !GameController.Instance.IsPaused;
        var not_blocking = AbilityLock.IsFree;
        var not_cooldown = !(ability.IsOnCooldown && !ability.CanPressWhileOnCooldown());
        var game_started = GameController.Instance.IsGameStarted;
        return not_blocking && not_cooldown && not_paused && game_started;
    }

    private void PressAbility(PlayerInput.ButtonType button)
    {
        if (InputLock.IsLocked) return;
        var ability = AbilityController.Instance.GetEquippedAbility(button);
        if (ability == null) return;
        if (GameStateController.Instance.GameState != GameStateType.PLAYING) return;

        if (CanPressAbility(ability))
        {
            ability.Pressed();
            AbilityQueued = null;
        }
        else
        {
            SoundController.Instance.Play(SoundEffectType.sfx_ability_cooldown);
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
            if (CanPressAbility(AbilityQueued))
            {
                AbilityQueued.Pressed();
                AbilityQueued = null;
            }
        }
    }

    public void UpdateBodyparts()
    {
        PlayerBody.ClearBodyparts();
        var equipped_abilities = AbilityController.Instance.GetEquippedAbilities();
        equipped_abilities.ForEach(ability => PlayerBody.CreateAbilityBodypart(ability.Info));

        foreach (var data in Save.PlayerBody.bodyparts)
        {
            var part = PlayerBody.CreateBodypart(data.type);

            part.SaveData = data;
            part.CounterPart.SaveData = data;

            part.SetPosition(data.position);
            part.SetSize(data.size);
        }
    }
    #endregion
    #region UPGRADES
    private void OnValuesUpdated()
    {
        UpdateUpgradeValues();
        UpdateBodyparts();
    }

    public void UpdateUpgradeValues()
    {
        ChanceToAvoidDamage = PlayerValueController.Instance.GetFloatValue(StatID.player_avoid_damage_chance);
        CollectRadius = COLLECT_RADIUS * PlayerValueController.Instance.GetFloatValue(StatID.player_collect_radius_perc);
        CollectCooldownReduction = PlayerValueController.Instance.GetFloatValue(StatID.player_collect_cooldown_flat);
        CollectSpeedBoost = PlayerValueController.Instance.GetBoolValue(StatID.player_collect_speed_perc);
        ConvertHealthToArmor = PlayerValueController.Instance.GetBoolValue(StatID.player_convert_health);
        Body.Size = Info.body_size * PlayerValueController.Instance.GetFloatValue(StatID.player_body_size_perc);
        InfiniteDrag = PlayerValueController.Instance.GetBoolValue(StatID.player_infinite_drag);
        KillEnemyShieldRegen = PlayerValueController.Instance.GetIntValue(StatID.player_regen_kill);
        PlantExpHealthRegen = PlayerValueController.Instance.GetIntValue(StatID.player_regen_plant);
        GlobalCooldownMultiplier = PlayerValueController.Instance.GetFloatValue(StatID.player_cooldown_multiplier);
        ExperienceMultiplier = PlayerValueController.Instance.GetFloatValue(StatID.player_exp_multiplier);
    }

    public void OnUpgradeUnlocked(UpgradeInfo info)
    {
        ps_upgrade.Play();

        foreach(var stat in info.upgrade.stats)
        {
            if(stat.id == StatID.player_health)
            {
                for (int i = 0; i < stat.value.GetIntValue(); i++)
                {
                    Health.AddHealth(HealthPoint.Type.FULL);
                }
            }
            else if (stat.id == StatID.player_armor)
            {
                for (int i = 0; i < stat.value.GetIntValue(); i++)
                {
                    Health.AddHealth(HealthPoint.Type.TEMPORARY);
                }
            }
            else if(stat.id == StatID.player_convert_health)
            {
                Health.ConvertHealthToArmor();
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
        if (hurt.CanHurt())
        {
            Damage(hurt.GetPosition());
        }
    }

    public static void PushEnemiesInArea(Vector3 position, float radius, float force, bool use_mass = false, AnimationCurve curve_force = null)
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
                enemy.Knockback(dir_knock, use_mass, false);
            }
        }
    }

    public bool TryKillEnemy(IKillable k)
    {
        var success = k.TryKill();

        if (success)
        {
            DecrementKillsUntilShieldRegen();
        }

        return success;
    }

    private void DecrementKillsUntilShieldRegen()
    {
        if (KillEnemyShieldRegen <= 0) return;

        enemy_kills_until_shield_regen--;
        if(enemy_kills_until_shield_regen <= 0)
        {
            SoundController.Instance.Play(SoundEffectType.sfx_gain_health);

            ResetKillsUntilShieldRegen();
            Health.AddHealth(HealthPoint.Type.TEMPORARY);
        }
    }

    private void ResetKillsUntilShieldRegen()
    {
        enemy_kills_until_shield_regen += KillEnemyShieldRegen;
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

        var init_health = Info.health + PlayerValueController.Instance.GetIntValue(StatID.player_health);
        for (int i = 0; i < init_health; i++) Health.AddHealth(HealthPoint.Type.FULL);

        var init_temp = Info.armor + PlayerValueController.Instance.GetIntValue(StatID.player_armor);
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
                SoundController.Instance.Play(SoundEffectType.sfx_player_damage);
            }
            else
            {
                PlayAvoidDamageFX();
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
        SoundController.Instance.Play(SoundEffectType.sfx_gain_health);
    }

    private IEnumerator PlayerDamageInvincibilityCr(float time)
    {
        InvincibilityLock.AddLock("Damage");
        yield return new WaitForSeconds(time);
        InvincibilityLock.RemoveLock("Damage");
        DamageIfInsideEnemy();
    }

    public void DamageIfInsideEnemy()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, Body.Trigger.radius);
        foreach (var hit in hits)
        {
            if (!hit.isTrigger) continue;
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
        SoundController.Instance.PlayGroup(SoundEffectType.sfx_collect_experience);
    }

    private void DecrementPlantExperienceUntilHealthRegen()
    {
        if (PlantExpHealthRegen <= 0) return;
        plant_exp_until_health_regen--;

        if (plant_exp_until_health_regen <= 0)
        {
            if (Health.HasHealth(HealthPoint.Type.EMPTY))
            {
                SoundController.Instance.Play(SoundEffectType.sfx_gain_health);
            }

            Health.Heal();
            ResetPlantExperienceUntilHealthRegen();
        }
    }

    private void ResetPlantExperienceUntilHealthRegen()
    {
        plant_exp_until_health_regen += PlantExpHealthRegen;
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
        var count = AbilityController.Instance.GetGainedAbilities().Count;
        if (count < 2)
        {
            return 5;
        }
        else if (count < 3)
        {
            return 7;
        }
        else if (count < 4)
        {
            return 10;
        }
        else
        {
            return 15;
        }
    }
    #endregion
    #region EFFECTS
    public void PlayLevelUpFX()
    {
        ps_level_up.Play();
        SoundController.Instance.Play(SoundEffectType.sfx_ui_level_up);
    }

    public void PlayCooldownCompleteFX()
    {
        ps_ability_off_cooldown.Play();
        SoundController.Instance.Play(SoundEffectType.sfx_ability_off_cooldown);
    }

    public void PlayAvoidDamageFX()
    {
        ps_avoid_damage.Play();
        SoundController.Instance.Play(SoundEffectType.sfx_avoid_damage);
    }
    #endregion
}
