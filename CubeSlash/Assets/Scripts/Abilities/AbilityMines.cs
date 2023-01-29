using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityMines : Ability
{
    [SerializeField] private Projectile prefab_mine;
    [SerializeField] private Projectile prefab_fragment;

    private int MineCount { get; set; }
    private int FragmentCount { get; set; }
    private float MineSize { get; set; }
    private float FragmentSize { get; set; }
    private float MineLifetime { get; set; }
    private float FragmentLifetime { get; set; }
    private float MineTurnSpeed { get; set; }
    private bool SeekingMines { get; set; }
    private bool ExplodingFragments { get; set; }
    private bool OnlyFragments { get; set; }
    private bool DoubleShells { get; set; }
    private bool FragmentChain { get; set; }

    private const float MINE_SPEED = 10f;
    private const float MINE_SPEED_SEEKING = 4f;
    private const float MINE_DRAG = 0.95f;
    private const float MINE_DRAG_SEEKING = 0.99f;
    private const float MINE_TURN_SEEKING = 1.5f;
    private const float MINE_SIZE = 0.8f;
    private const float MINE_LIFETIME = 2f;
    private const float FRAGMENT_SIZE = 0.5f;
    private const float FRAGMENT_SPEED = 20f;
    private const float FRAGMENT_DRAG = 0.95f;
    private const float FRAGMENT_LIFETIME = 0.5f;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
    }

    public override void OnValuesApplied()
    {
        base.OnValuesApplied();

        MineCount = GetIntValue("MineCount");
        FragmentCount = GetIntValue("FragmentCount");
        MineSize = MINE_SIZE * GetFloatValue("MineSize");
        FragmentSize = FRAGMENT_SIZE * GetFloatValue("FragmentSize");
        MineLifetime = MINE_LIFETIME * GetFloatValue("MineLifetime");
        FragmentLifetime = FRAGMENT_LIFETIME * GetFloatValue("FragmentLifetime");
        MineTurnSpeed = MINE_TURN_SEEKING * GetFloatValue("MineTurnSpeed");
        SeekingMines = GetBoolValue("SeekingMines");
        ExplodingFragments = GetBoolValue("ExplodingFragments");
        OnlyFragments = GetBoolValue("OnlyFragments");
        DoubleShells = GetBoolValue("DoubleShells");
        FragmentChain = GetBoolValue("FragmentChain");
    }

    public override void Trigger()
    {
        base.Trigger();

        if (OnlyFragments)
        {
            var position = Player.transform.position;
            var count = GetMineCount() * FragmentCount;

            if (FragmentChain)
            {
                ChainFragments(position, count);
            }
            else
            {
                var ps = ShootFragments(position, prefab_fragment, count, FRAGMENT_SPEED, FragmentSize);
                foreach (var p in ps)
                {
                    SetupMineFragment(p);
                }
            }

            StartCooldown();
        }
        else
        {
            ShootMines(GetMineCount());
        }
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
        var speed = SeekingMines ? MINE_SPEED_SEEKING : MINE_SPEED;

        var p = ProjectileController.Instance.ShootPlayerProjectile(new ProjectileController.PlayerShootInfo
        {
            prefab = prefab_mine,
            position_start = Player.transform.position,
            velocity = direction.normalized * speed,
            onHit = OnMineExplode
        });

        p.transform.localScale = Vector3.one * MineSize;
        p.Drag = SeekingMines ? MINE_DRAG_SEEKING : MINE_DRAG;
        p.Lifetime = MINE_LIFETIME;
        p.Homing = SeekingMines;
        p.SearchRadius = 100f;
        p.TurnSpeed = SeekingMines ? MineTurnSpeed : 0;
        p.onDeath += () => OnMineExplode(p);
    }

    private void OnMineExplode(Projectile projectile, IKillable k = null)
    {
        if (ExplodingFragments)
        {
            AbilityExplode.Explode(projectile.transform.position, 3f, 0);
        }

        if (FragmentChain)
        {
            ChainFragments(projectile.transform.position, FragmentCount);
        }
        else
        {
            var ps = ShootFragments(projectile.transform.position, prefab_fragment, FragmentCount, FRAGMENT_SPEED, FragmentSize);
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
        p.Lifetime = FRAGMENT_LIFETIME * Random.Range(0.8f, 1.2f);
        p.Rigidbody.velocity = p.Rigidbody.velocity.normalized * p.Rigidbody.velocity.magnitude * Random.Range(0.8f, 1f);
        p.Drag = FRAGMENT_DRAG;

        p.onHit += c => OnCollide(p, c);
        p.onDeath += () => OnFragment(p);

        void OnCollide(Projectile p, Collider2D c)
        {
            var k = c.GetComponentInParent<IKillable>();
            if(k != null)
            {
                OnFragment(p);
            }
        }

        void OnFragment(Projectile p)
        {
            if (ExplodingFragments)
            {
                AbilityExplode.Explode(p.transform.position, 2f, 0);
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
        return DoubleShells ? MineCount * 2 : MineCount;
    }
}