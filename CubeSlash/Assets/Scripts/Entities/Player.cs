using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviourExtended
{
    public static Player Instance;
    public Character Character { get { return GetComponentOnce<Character>(ComponentSearchType.CHILDREN); } }
    public Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.CHILDREN); } }
    public MinMaxInt Experience { get; private set; } = new MinMaxInt();
    public MinMaxInt Health { get; private set; } = new MinMaxInt();
    public int Level { get; private set; }
    public MultiLock InputLock { get; set; } = new MultiLock();
    public MultiLock InvincibilityLock { get; set; } = new MultiLock();
    public MultiLock AbilityLock { get; set; } = new MultiLock();
    public MultiLock MovementLock { get; set; } = new MultiLock();
    public MultiLock DragLock { get; set; } = new MultiLock();
    public Vector3 MoveDirection { get; set; }

    public const float SPEED_MOVE = 5;

    public System.Action<Enemy> onEnemyKilled;
    public System.Action<Enemy> onHurt;

    public Ability[] AbilitiesEquipped { get; private set; }
    public List<Ability> AbilitiesUnlocked { get; private set; } = new List<Ability>();
    public Ability AbilityQueued { get; private set; }

    public void Initialize()
    {
        Health.Min = 0;
        Health.Max = 3;
        Health.Value = Health.Max;
        Experience.Min = 0;
        Experience.Max = 25;
        Experience.Value = Experience.Min;
        AbilitiesEquipped = new Ability[PlayerInputController.Instance.CountAbilityButtons];
        var dash = UnlockAbility(Ability.Type.DASH);
        var split = UnlockAbility(Ability.Type.SPLIT);
        var charge = UnlockAbility(Ability.Type.CHARGE);
        EquipAbility(dash, 2);
        EquipAbility(split, 0);
        EquipAbility(charge, 1);

        Character.Initialize();

        MoveDirection = transform.up;
    }

    private void Update()
    {
        MoveUpdate();
        QueuedAbilityUpdate();
    }
    #region ABILITIES
    public void InitializeAbilities()
    {
        foreach(var ability in AbilitiesEquipped.Where(a => a != null))
        {
            ability.InitializeActive();
        }
    }

    public void EquipAbility(Ability ability, int idx)
    {
        // Unequip previous
        UnequipAbility(idx);

        // Equip next
        AbilitiesEquipped[idx] = ability;
        if(ability != null)
        {
            ability.Equipped = true;
        }
    }

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

    public Ability UnlockAbility(Ability.Type type)
    {
        //var already_unlocked = AbilitiesUnlocked.Any(ability => ability.type == type);
        //if (already_unlocked) return null;

        var prefab = Ability.GetPrefab(type);
        var ability = Instantiate(prefab.gameObject).GetComponent<Ability>();
        if (ability)
        {
            AbilitiesUnlocked.Add(ability);
            ability.transform.parent = transform;
            ability.transform.position = transform.position;
            ability.transform.rotation = transform.rotation;
            ability.Player = this;
        }
        return ability;
    }

    public void UnlockAllAbilities()
    {
        var types = System.Enum.GetValues(typeof(Ability.Type)).Cast<Ability.Type>().ToArray();
        foreach(var type in types)
        {
            UnlockAbility(type);
        }
    }

    private bool CanUseAbility(Ability ability)
    {
        var not_blocking = AbilityLock.IsFree;
        var not_cooldown = !ability.OnCooldown;
        return not_blocking && not_cooldown;
    }

    public void PressAbility(int idx)
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
            else if(ability.TimeCooldownLeft < 0.5f)
            {
                AbilityQueued = ability;
            }
        }
    }

    public void ReleaseAbility(int idx)
    {
        if (InputLock.IsLocked) return;

        var ability = AbilitiesEquipped[idx];
        if (ability)
        {
            if (ability.IsPressed)
            {
                ability.Released();
            }
            
            if(AbilityQueued == ability)
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
    #endregion
    #region MOVE
    private void MoveUpdate()
    {
        var hor = Input.GetAxis("Horizontal");
        var ver = Input.GetAxis("Vertical");
        var dir = new Vector2(hor, ver);
        if(MovementLock.IsFree && InputLock.IsFree && dir.magnitude > 0.5f)
        {
            MoveDirection = dir.normalized;
            Move(dir.normalized);
        }
        else if(DragLock.IsFree)
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
            Experience.Value += 1;
            onEnemyKilled?.Invoke(enemy);
        }
        else
        {

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var enemy = collision.gameObject.GetComponentInParent<Enemy>();
        if (enemy)
        {
            foreach(var ability in AbilitiesEquipped)
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

        if(Health.Value > 0)
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
        foreach(var hit in hits)
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
        while(Time.time < time_end)
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
        while(Time.time < time_end)
        {
            Rigidbody.velocity = Rigidbody.velocity * 0.99f;
            yield return null;
        }

        MovementLock.RemoveLock("Damage");
        DragLock.RemoveLock("Damage");
    }
    #endregion
}
