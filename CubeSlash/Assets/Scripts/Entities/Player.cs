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
    public int Level { get; private set; }
    public bool InputEnabled { get; set; }
    public int InvincibilityCounter { get; set; }
    public Vector3 MoveDirection { get; set; }

    public const float SPEED_MOVE = 5;

    public System.Action<Enemy> onEnemyKilled;
    public System.Action<Enemy> onHurt;

    public Ability[] AbilitiesEquipped { get; private set; }
    private List<Ability> abilities_unlocked = new List<Ability>();
    private string[] buttons_ability = new string[] { "Fire1", "Fire2", "Fire3", "Jump" };

    public void Initialize()
    {
        Experience.Min = 0;
        Experience.Max = 25;
        Experience.Value = 0;
        AbilitiesEquipped = new Ability[buttons_ability.Length];
        var dash = UnlockAbility(Ability.Type.DASH);
        EquipAbility(dash, 2);
    }

    private void Update()
    {
        AbilityUpdate();
        MoveUpdate();
    }
    #region ABILITIES
    public void EquipAbility(Ability ability, int idx)
    {
        AbilitiesEquipped[idx] = ability;
        ability.Equipped = true;
    }

    public Ability UnlockAbility(Ability.Type type)
    {
        var already_unlocked = abilities_unlocked.Any(ability => ability.type == type);
        if (already_unlocked) return null;

        var ability = Ability.Create(type);
        abilities_unlocked.Add(ability);
        ability.transform.parent = transform;
        ability.Player = this;
        return ability;
    }

    private bool CanUseAbilities()
    {
        var no_abilities = !AbilitiesEquipped.Any(ability => ability != null && ability.BlockingAbilities);
        return no_abilities;
    }

    private void AbilityUpdate()
    {
        if (!InputEnabled) return;
        if (CanUseAbilities())
        {
            for (int i = 0; i < AbilitiesEquipped.Length; i++)
            {
                var button = buttons_ability[i];
                var ability = AbilitiesEquipped[i];
                if (ability)
                {
                    if (Input.GetButtonDown(button))
                    {
                        ability.Pressed();
                    }

                    if (Input.GetButtonUp(button))
                    {
                        ability.Released();
                    }
                }
            }
        }
    }
    #endregion
    #region MOVE
    private void MoveUpdate()
    {
        if (!CanMove()) return;

        var hor = Input.GetAxis("Horizontal");
        var ver = Input.GetAxis("Vertical");
        var dir = new Vector2(hor, ver);
        if(InputEnabled && dir.magnitude > 0.5f)
        {
            MoveDirection = dir.normalized;
            Move(dir.normalized);
        }
        else
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

    private bool CanMove()
    {
        var no_abilities = !AbilitiesEquipped.Any(ability => ability != null && ability.BlockingMovement);
        return no_abilities;
    }
    #endregion
    #region ENEMY
    public void DamageEnemy(Enemy enemy, int damage)
    {
        enemy.Damage(damage);

        if(enemy.health > 0)
        {
            InstantiateParticle("Particles/ps_impact_dash")
                        .Position(enemy.transform.position)
                        .Destroy(1)
                        .Play();
        }
        else
        {
            Experience.Value += 1;
            onEnemyKilled?.Invoke(enemy);
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

            if (InvincibilityCounter > 0)
            {
                
            }
            else
            {
                print("Player hit by enemy");
                onHurt?.Invoke(enemy);
            }
        }
    }
    #endregion
}
