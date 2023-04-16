using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityMines : Ability
{
    [Header("MINES")]
    [SerializeField] private MinesProjectile prefab_mine;
    [SerializeField] private Projectile prefab_fragment;
    [SerializeField] private DamageTrail trail;

    private float Cooldown { get; set; }
    private int ShellCount { get; set; }
    private int FragmentCount { get; set; }
    private float ShellSize { get; set; }
    private float FragmentSize { get; set; }
    private float ShellLifetime { get; set; }
    private float FragmentLifetime { get; set; }
    private bool ExplodingFragments { get; set; }
    private bool DoubleShells { get; set; }
    private bool FragmentChain { get; set; }
    private bool Trail { get; set; }

    private const float MINE_SPEED = 10f;
    private const float MINE_DRAG = 0.95f;
    private const float MINE_SIZE = 0.8f;
    private const float MINE_LIFETIME = 2f;
    private const float FRAGMENT_SIZE = 0.5f;
    private const float FRAGMENT_SPEED = 20f;
    private const float FRAGMENT_DRAG = 0.95f;
    private const float FRAGMENT_LIFETIME = 0.5f;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
        trail.gameObject.SetActive(false);
    }

    public override void OnValuesUpdated()
    {
        base.OnValuesUpdated();

        Cooldown = GetFloatValue(StatID.mines_cooldown_flat) * GetFloatValue(StatID.mines_cooldown_perc);
        ShellCount = GetIntValue(StatID.mines_shell_count);
        FragmentCount = GetIntValue(StatID.mines_fragment_count);
        ShellSize = MINE_SIZE * GetFloatValue(StatID.mines_size_perc);
        FragmentSize = FRAGMENT_SIZE * GetFloatValue(StatID.mines_fragment_size_perc);
        ShellLifetime = MINE_LIFETIME * GetFloatValue(StatID.mines_shell_lifetime_perc);
        FragmentLifetime = FRAGMENT_LIFETIME * GetFloatValue(StatID.mines_fragment_lifetime_perc);
        ExplodingFragments = GetBoolValue(StatID.mines_fragment_explode);
        DoubleShells = GetBoolValue(StatID.mines_double_shell);
        FragmentChain = GetBoolValue(StatID.mines_fragment_chain);
        Trail = GetBoolValue(StatID.mines_trail);
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

            var delta = 20;
            for (int i = 0; i < count; i++)
            {
                var mul = i % 2 == 0 ? 1 : -1;
                var second = 1 + i / 2;
                var back = -Player.Body.transform.up;
                var angle = i == 0 ? 0 : delta * second * mul;
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
            velocity = direction.normalized * MINE_SPEED,
            onKill = (p, k) => OnMineHit(p.transform.position)
        }) as MinesProjectile;

        p.transform.localScale = Vector3.one * ShellSize;
        p.Drag = MINE_DRAG;
        p.Lifetime = ShellLifetime;
        p.onDeath += () => OnMineHit(p.transform.position);
        p.onChainHit += OnMineHit;

        p.HasChain = FragmentChain;
        p.HasTrail = Trail;
        p.ChainHits = FragmentCount;
    }

    private void OnMineHit(Vector3 position)
    {
        SoundController.Instance.Play(SoundEffectType.sfx_mines_explode);

        if (ExplodingFragments)
        {
            var radius = 3f * ShellSize;
            AbilityExplode.Explode(position, radius, 0);
        }

        if (Trail)
        {
            trail.radius = 1f;
            trail.lifetime = 2f;
            trail.CreateTrail(position);
        }

        if (FragmentChain)
        {
            ChainFragments(position, FragmentCount);
        }
        else
        {
            var ps = ShootFragments(position, prefab_fragment, FragmentCount, FRAGMENT_SPEED, FragmentSize);
            foreach (var p in ps)
            {
                SetupMineFragment(p);
            }
        }
    }

    private void ChainFragments(Vector3 position, int count)
    {
        AbilityChain.CreateImpactPS(position);
        AbilityChain.TryChainToTarget(position, 5f, 1, count, 0);
    }

    private void SetupMineFragment(Projectile p)
    {
        p.Lifetime = FragmentLifetime * Random.Range(0.8f, 1.2f);
        p.Rigidbody.velocity = p.Rigidbody.velocity.normalized * p.Rigidbody.velocity.magnitude * Random.Range(0.8f, 1f);
        p.Drag = FRAGMENT_DRAG;

        p.onHitEnemy += k => OnFragment(p);
        p.onDeath += () => OnFragment(p);

        void OnFragment(Projectile p)
        {
            if (ExplodingFragments)
            {
                var radius = 2f * FragmentSize;
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