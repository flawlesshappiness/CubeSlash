using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviourExtended
{
    public static Player Instance;
    public PlayerSettings settings;
    public Character Character { get { return GetComponentOnce<Character>(ComponentSearchType.CHILDREN); } }
    public Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.CHILDREN); } }
    public MinMaxInt Experience { get; private set; } = new MinMaxInt();
    public MinMaxInt Health { get; private set; } = new MinMaxInt();
    public int Level { get; private set; }
    public int AbilityPoints { get; set; }
    public MultiLock InputLock { get; set; } = new MultiLock();
    public MultiLock InvincibilityLock { get; set; } = new MultiLock();
    public MultiLock AbilityLock { get; set; } = new MultiLock();
    public MultiLock MovementLock { get; set; } = new MultiLock();
    public MultiLock DragLock { get; set; } = new MultiLock();
    public Vector3 MoveDirection { get; set; }
    public float DistanceCollect { get; private set; } = 3f;

    public const float SPEED_MOVE = 5;

    public System.Action<Enemy> onEnemyKilled;
    public System.Action<Enemy> onHurt;

    public Ability[] AbilitiesEquipped { get; private set; }
    public List<Ability> AbilitiesUnlocked { get; private set; } = new List<Ability>();
    public Ability AbilityQueued { get; private set; }

    private PlayerInput.ButtonType[] ability_input_map = new PlayerInput.ButtonType[]
    {
        PlayerInput.ButtonType.SOUTH,
        PlayerInput.ButtonType.EAST,
        PlayerInput.ButtonType.WEST,
        PlayerInput.ButtonType.NORTH,
    };

    public void Initialize()
    {
        Health.Min = 0;
        Health.Max = 3;
        Health.Value = Health.Max;

        Experience.Min = 0;
        Experience.Max = 5;
        Experience.Value = Experience.Min;
        Experience.onMax += OnLevelUp;

        AbilitiesEquipped = new Ability[ConstVars.COUNT_ABILITY_BUTTONS];
        var dash = UnlockAbility(Ability.Type.DASH);
        var split = UnlockAbility(Ability.Type.SPLIT);
        var charge = UnlockAbility(Ability.Type.CHARGE);
        EquipAbility(dash, 2);
        //EquipAbility(split, 0);
        //EquipAbility(charge, 1);

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
    public void InitializeAbilities()
    {
        foreach (var ability in AbilitiesEquipped.Where(a => a != null))
        {
            ability.InitializeActive();
        }
    }

    public void EquipAbility(Ability ability, PlayerInput.ButtonType type) => EquipAbility(ability, AbilityInputToIndex(type));
    public void EquipAbility(Ability ability, int idx)
    {
        // Unequip previous
        UnequipAbility(idx);

        // Equip next
        AbilitiesEquipped[idx] = ability;
        if (ability != null)
        {
            ability.Equipped = true;
        }
    }

    public void UnequipAbility(PlayerInput.ButtonType type) => UnequipAbility(AbilityInputToIndex(type));
    public void UnequipAbility(int idx)
    {
        var ability = AbilitiesEquipped[idx];
        if (ability)
        {
            ability.Equipped = false;
            AbilitiesEquipped[idx] = null;

            for (int i = 0; i < ability.Modifiers.Length; i++)
            {
                var modifier = ability.Modifiers[i];
                if (modifier)
                {
                    ability.Modifiers[i] = null;
                    modifier.Equipped = false;
                }
            }
        }
    }

    public Ability GetEquippedAbility(PlayerInput.ButtonType type) => AbilitiesEquipped[AbilityInputToIndex(type)];
    
    public Ability UnlockAbility(Ability.Type type)
    {
        var prefab = Ability.GetPrefab(type);
        var ability = Instantiate(prefab.gameObject).GetComponent<Ability>();
        if (ability)
        {
            AbilitiesUnlocked.Add(ability);
            ability.transform.parent = transform;
            ability.transform.position = transform.position;
            ability.transform.rotation = transform.rotation;
            ability.Player = this;
            ability.InitializeFirstTime();
        }
        return ability;
    }

    public void UnlockAllAbilities()
    {
        var types = System.Enum.GetValues(typeof(Ability.Type)).Cast<Ability.Type>().ToArray();
        foreach (var type in types)
        {
            UnlockAbility(type);
        }
    }

    private bool CanUseAbility(Ability ability)
    {
        var not_paused = !GameController.Instance.IsPaused;
        var not_blocking = AbilityLock.IsFree;
        var not_cooldown = !ability.OnCooldown;
        return not_blocking && not_cooldown && not_paused;
    }

    private void PressAbility(PlayerInput.ButtonType type) => PressAbility(AbilityInputToIndex(type));
    private void PressAbility(int idx)
    {
        if (InputLock.IsLocked) return;
        var ability = AbilitiesEquipped[idx];

        if (ability)
        {
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
    }

    private void ReleaseAbility(PlayerInput.ButtonType type) => ReleaseAbility(AbilityInputToIndex(type));
    private void ReleaseAbility(int idx)
    {
        if (InputLock.IsLocked) return;

        var ability = AbilitiesEquipped[idx];
        if (ability)
        {
            if (ability.IsPressed)
            {
                ability.Released();
            }

            if (AbilityQueued == ability)
            {
                AbilityQueued = null;
            }
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

    private int AbilityInputToIndex(PlayerInput.ButtonType type)
    {
        return ability_input_map.ToList().IndexOf(type);
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
        Rigidbody.velocity = direction * SPEED_MOVE;
        Character.SetLookDirection(direction);
    }
    #endregion
    #region ENEMY
    public void KillEnemy(Enemy enemy)
    {
        if (enemy.IsKillable())
        {
            enemy.Kill();
            onEnemyKilled?.Invoke(enemy);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var enemy = collision.gameObject.GetComponentInParent<Enemy>();
        if (enemy)
        {
            foreach (var ability in AbilitiesEquipped)
            {
                if (!ability) continue;
                ability.EnemyCollision(enemy);
            }

            if (InvincibilityLock.IsLocked)
            {

            }
            else
            {
                Damage(1, enemy.transform.position);
                onHurt?.Invoke(enemy);
            }
        }
    }
    #endregion
    #region HEALTH
    public void Kill()
    {
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
    }

    public void Damage(int amount, Vector3 damage_origin)
    {
        Health.Value -= amount.Abs();

        if (Health.Value > 0)
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
        Level++;
        AbilityPoints++;
    }

    public void ResetExperience()
    {
        var t_level = Level / (float)settings.experience_level_max;
        var t_exp = settings.curve_experience.Evaluate(t_level);
        Experience.Max = (int)(Mathf.Lerp(settings.experience_min, settings.experience_max, t_exp));
        Experience.Value = Experience.Min;
    }
    #endregion
}
