using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityMines : Ability
{
    [Header("MINES")]
    [SerializeField] private MinesProjectile prefab_mine;
    [SerializeField] private MinesFragmentProjectile prefab_fragment;
    [SerializeField] private DamageTrail trail;

    private float Cooldown { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.mines_cooldown).ModifiedValue.float_value; } }
    private int ShellCount { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.mines_shell_count).ModifiedValue.int_value; } }
    private int FragmentCount { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.mines_fragment_count).ModifiedValue.int_value; } }
    private bool ExplodingFragments { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.mines_explode).ModifiedValue.bool_value; } }
    private bool DoubleShells { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.mines_double_shell).ModifiedValue.bool_value; } }
    private bool FragmentChain { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.mines_chain).ModifiedValue.bool_value; } }
    private bool FragmentsPierce { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.mines_fragment_pierce).ModifiedValue.bool_value; } }
    private bool FragmentsCurve { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.mines_fragment_curve).ModifiedValue.bool_value; } }
    private float FragmentLifetime { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.mines_fragment_lifetime).ModifiedValue.float_value; } }

    private const float SHELL_SPEED = 10f;
    private const float SHELL_DRAG = 0.95f;
    private const float SHELL_SIZE = 0.8f;
    private const float SHELL_LIFETIME = 2f;
    private const float FRAGMENT_SIZE = 0.5f;
    private const float FRAGMENT_SPEED = 20f;
    private const float FRAGMENT_DRAG = 0.95f;
    private const float FRAGMENT_DRAG_PIERCE = 0.98f;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
        trail.gameObject.SetActive(false);
    }

    public override float GetBaseCooldown() => Cooldown;

    public override void Trigger()
    {
        base.Trigger();
        ShootMines(GetMineCount());
    }

    private void ShootMines(int count)
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            Player.AbilityLock.AddLock(nameof(AbilityMines));

            var angle_max = 45;
            for (int i = 0; i < count; i++)
            {
                var mul = i % 2 == 0 ? 1 : -1;
                var sine_val = i * 0.3f;
                var back = -Player.Body.transform.up;
                var angle = Mathf.Sin(sine_val) * angle_max * mul;
                var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                var direction = rotation * back;
                ShootMine(direction);
                yield return new WaitForSeconds(0.1f);
            }

            Player.AbilityLock.RemoveLock(nameof(AbilityMines));
            StartCooldown();
        }
    }

    private void ShootMine(Vector3 direction)
    {
        SoundController.Instance.Play(SoundEffectType.sfx_mines_spawn);

        var p = ProjectileController.Instance.ShootPlayerProjectile(new ProjectileController.PlayerShootInfo
        {
            prefab = prefab_mine,
            position_start = Player.transform.position,
            velocity = direction.normalized * SHELL_SPEED,
        }) as MinesProjectile;

        p.transform.localScale = Vector3.one * SHELL_SIZE;
        p.Drag = SHELL_DRAG;
        p.Lifetime = SHELL_LIFETIME;
        p.onDestroy += () => OnMineHit(p.transform.position);
        p.onDestroy += () => OnMineDestroy(p);
        p.onChainHit += OnMineHit;

        p.HasChain = FragmentChain;
        p.HasTrail = false;
        p.ChainHits = FragmentCount;
    }

    private void OnMineHit(Vector3 position)
    {
        if (ExplodingFragments)
        {
            var radius = 3f * SHELL_SIZE;
            AbilityExplode.Explode(position, radius, 0);
        }

        if (!FragmentChain)
        {
            var ps = ShootFragments(position, prefab_fragment, FragmentCount, FRAGMENT_SPEED, FRAGMENT_SIZE);
            foreach (var p in ps)
            {
                SetupMineFragment(p as MinesFragmentProjectile);
            }
        }
    }

    private void OnMineDestroy(MinesProjectile p)
    {
        SoundController.Instance.PlayGroup(SoundEffectType.sfx_mines_explode);
        SoundController.Instance.SetGroupVolumeByPosition(SoundEffectType.sfx_mines_explode, p.transform.position);
    }

    private void ChainFragments(Vector3 position, int count)
    {
        AbilityChain.CreateImpactPS(position);
        AbilityChain.TryChainToTarget(new AbilityChain.ChainInfo
        {
            center = position,
            radius = 5f,
            chains_left = 1,
            initial_strikes = count
        });
    }

    private void SetupMineFragment(MinesFragmentProjectile p)
    {
        p.Piercing = FragmentsPierce ? -1 : 0;
        p.Lifetime = FragmentLifetime * Random.Range(0.8f, 1.2f);
        p.Rigidbody.velocity = p.Rigidbody.velocity.normalized * p.Rigidbody.velocity.magnitude * Random.Range(0.8f, 1f);
        p.Drag = FragmentsPierce ? FRAGMENT_DRAG_PIERCE : FRAGMENT_DRAG;
        p.CurveAngle = FragmentsCurve ? 200 : 0;

        p.onHitEnemy += k => OnFragment(p);
        p.onDeath += () => OnFragment(p);

        void OnFragment(Projectile p)
        {
            if (ExplodingFragments)
            {
                var radius = 4f * FRAGMENT_SIZE;
                AbilityExplode.Explode(p.transform.position, radius, 0);
            }
        }
    }

    public static List<Projectile> ShootFragments(Vector3 position, Projectile prefab, int count, float speed, float size)
    {
        var projectiles = new List<Projectile>();

        var angle_per = 360f / count;
        var angle_min = angle_per * 0.5f;
        var angle_max = angle_per;
        var angle = 0f;
        var start_direction = Random.insideUnitCircle.ToVector3().normalized;
        for (int i = 0; i < count; i++)
        {
            var angle_delta = Random.Range(angle_min, angle_max);
            angle += angle_delta;
            var angle_diff = angle_per - angle_delta;
            angle_max += angle_diff;
            var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            var direction = rotation * start_direction;
            var velocity = direction * speed;
            var p = ShootFragment(prefab, position, velocity, size);

            projectiles.Add(p);
        }

        return projectiles;
    }

    public static Projectile ShootFragment(Projectile prefab, Vector3 position, Vector3 velocity, float size)
    {
        var p = ProjectileController.Instance.ShootPlayerProjectile(new ProjectileController.PlayerShootInfo
        {
            prefab = prefab,
            position_start = position,
            velocity = velocity,
        });

        p.transform.localScale = Vector3.one * size;
        return p;
    }

    private int GetMineCount()
    {
        return DoubleShells ? ShellCount * 2 : ShellCount;
    }
}