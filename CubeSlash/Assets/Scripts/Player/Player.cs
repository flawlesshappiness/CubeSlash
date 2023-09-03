using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : Character
{
    public static Player Instance;
    [SerializeField] private Transform camera_target;
    [SerializeField] private PlayerDodge dodge;
    [SerializeField] public PlayerHeal heal;
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

    public event System.Action onLevelUp;
    public event System.Action onDeath;
    public event System.Action onHurt;
    public event System.Action onEnemyKilled;
    public event System.Action<Collider2D> onTriggerEnter;
    public event System.Action onValuesUpdated;

    private GameAttribute att_velocity;
    private GameAttribute att_acceleration;
    private GameAttribute att_chance_avoid_damage;
    private GameAttribute att_experience_multiplier;

    public void Initialize()
    {
        GameController.Instance.onMainMenu += OnMainMenu;
        GameController.Instance.onGameStart += OnGameStart;

        // Experience
        Experience.onMax += OnLevelUp;

        // Upgrades
        UpgradeController.Instance.onUpgradeUnlocked += OnUpgradeUnlocked;

        // Setup
        MoveDirection = transform.up;
        g_invincible.SetActive(false);
        CameraController.Instance.Target = camera_target;

        Clear();
    }

    private void OnMainMenu()
    {
        SetRigidbodyEnabled(false);
        Clear();
    }

    private void OnGameStart()
    {
        SetRigidbodyEnabled(true);
    }

    public void Clear()
    {
        AbilityController.Instance.Clear();
        UpgradeController.Instance.ClearUpgrades();
        heal.Clear();

        ResetPlayerBody();
        SetPrimaryAbility(Save.PlayerBody.primary_ability);
        UpdateBodyparts();
        UpdateGameAttributes();

        gameObject.SetActive(true);
        g_invincible.SetActive(false);
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

    public void SetPrimaryAbility(Ability.Type type)
    {
        AbilityController.Instance.Clear();

        Save.PlayerBody.primary_ability = type;
        AbilityController.Instance.GainAbility(type);
        AbilityController.Instance.SetEquippedAbility(type);
    }

    public void ResetValues()
    {
        Level = 0;
        IsDead = false;
        ResetHealth();
        ResetExperience();
        ResetLevelsUntilAbility();
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
        UpdateCameraTarget();
        UpdateDodgeCooldown();
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

        LinearVelocity = att_velocity.ModifiedValue.float_value;
        LinearAcceleration = att_acceleration.ModifiedValue.float_value;
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
            else
            {
                MoveToStop();
            }
        }
    }

    private void SetRigidbodyEnabled(bool enabled)
    {
        Rigidbody.isKinematic = !enabled;

        if (!enabled)
        {
            Rigidbody.velocity = Vector3.zero;
        }
    }

    private void UpdateGameAttributes()
    {
        att_velocity = GameAttributeController.Instance.GetAttribute(GameAttributeType.player_velocity);
        att_acceleration = GameAttributeController.Instance.GetAttribute(GameAttributeType.player_acceleration);
        att_chance_avoid_damage = GameAttributeController.Instance.GetAttribute(GameAttributeType.player_avoid_damage_chance);
        att_experience_multiplier = GameAttributeController.Instance.GetAttribute(GameAttributeType.player_exp_multiplier);
    }

    private void UpdateDodgeCooldown()
    {
        PlayerBody.SetCooldown(dodge.GetCooldownPercentage());
    }

    #region ABILITIES
    private bool CanPressAbility(Ability ability)
    {
        var not_blocking = AbilityLock.IsFree;
        var not_cooldown = !(ability.IsOnCooldown && !ability.CanPressWhileOnCooldown());
        return not_blocking && not_cooldown;
    }

    private void PressAbility(PlayerInput.ButtonType button)
    {
        if (!GameController.Instance.IsGameStarted) return;
        if (GameController.Instance.IsPaused) return;
        if (InputLock.IsLocked) return;
        switch (button)
        {
            case PlayerInput.ButtonType.WEST:
                PressEquippedAbility();
                break;

            case PlayerInput.ButtonType.EAST:
                dodge.Press();
                break;

            case PlayerInput.ButtonType.NORTH:
                heal.Press();
                break;
        }
    }

    private void PressEquippedAbility()
    {
        if (GameStateController.Instance.GameState != GameStateType.PLAYING) return;

        var ability = AbilityController.Instance.GetEquippedAbility();
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
        if (!GameController.Instance.IsGameStarted) return;
        if (GameController.Instance.IsPaused) return;
        if (InputLock.IsLocked) return;

        switch (button)
        {
            case PlayerInput.ButtonType.WEST:
                ReleaseEquippedAbility();
                break;
        }
    }

    private void ReleaseEquippedAbility()
    {
        var ability = AbilityController.Instance.GetEquippedAbility();

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
        var equipped_ability = AbilityController.Instance.GetEquippedAbility();
        var bdp = PlayerBody.CreateAbilityBodypart(equipped_ability.Info);
        equipped_ability.SetBodypart(bdp);

        foreach (var data in Save.PlayerBody.bodyparts)
        {
            var part = PlayerBody.CreateBodypart(data.type);

            part.SaveData = data;
            part.CounterPart.SaveData = data;

            part.SetPosition(data.position);
            part.SetSize(data.size);
            part.SetMirrored(data.mirrored);
        }
    }
    #endregion
    #region UPGRADES
    public void OnUpgradeUnlocked(UpgradeInfo info)
    {
        ps_upgrade.Play();

        foreach (var modif in info.upgrade.modifiers)
        {
            if (modif.attribute_type == GameAttributeType.player_health)
            {
                for (int i = 0; i < modif.int_value; i++)
                {
                    Health.AddHealth(HealthPoint.Type.FULL);
                }
            }
            else if (modif.attribute_type == GameAttributeType.player_armor)
            {
                for (int i = 0; i < modif.int_value; i++)
                {
                    Health.AddHealth(HealthPoint.Type.TEMPORARY);
                }
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
            onEnemyKilled?.Invoke();
        }

        return success;
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

        var init_health = Info.health;
        for (int i = 0; i < init_health; i++) Health.AddHealth(HealthPoint.Type.FULL);

        var init_temp = Info.armor;
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
            var chance = att_chance_avoid_damage.ModifiedValue.float_value;
            if (chance == 0 || Random.Range(0f, 1f) > chance)
            {
                Health.Damage();
                SoundController.Instance.Play(SoundEffectType.sfx_player_damage);
            }
            else
            {
                PlayAvoidDamageFX();
            }

            onHurt?.Invoke();
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

    public void AddHealth(HealthPoint.Type type)
    {
        // Heal
        if (type == HealthPoint.Type.TEMPORARY)
        {
            Health.AddHealth(HealthPoint.Type.TEMPORARY);
        }
        else if (type == HealthPoint.Type.FULL)
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
        var mul_difficulty = GameSettings.Instance.experience_mul_difficulty.Evaluate(DifficultyController.Instance.DifficultyValue);
        Experience.Value += 1f * att_experience_multiplier.ModifiedValue.float_value * mul_difficulty;

        if (type == ExperienceType.PLANT)
        {
            ps_collect_plant.Play();
        }
        else if (type == ExperienceType.MEAT)
        {
            ps_collect_meat.Play();
        }

        // Sound
        SoundController.Instance.PlayGroup(SoundEffectType.sfx_collect_experience);
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

    public int GetMaxLevelsUntilAbility() => 8;
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
    #region CAMERA
    private void UpdateCameraTarget()
    {
        var distance = Rigidbody.velocity.magnitude * 0.3f;
        camera_target.localPosition = Rigidbody.velocity.normalized * distance;
    }
    #endregion
}
