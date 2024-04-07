using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityOrbit : Ability
{
    [SerializeField] private OrbitProjectile prefab_projectile;

    private float RingRadius { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.orbit_ring_radius).ModifiedValue.float_value; } }
    private int RingCount { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.orbit_ring_count).ModifiedValue.int_value; } }
    private int ProjectileCount { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.orbit_projectile_count).ModifiedValue.int_value; } }
    private float ProjectileSize { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.orbit_projectile_size).ModifiedValue.float_value; } }
    private float ProjectileOrbitTime { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.orbit_projectile_time).ModifiedValue.float_value; } }
    private bool Explode { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.orbit_explode).ModifiedValue.bool_value; } } // Changes size instead
    private bool Split { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.orbit_split).ModifiedValue.bool_value; } }
    private bool Chain { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.orbit_chain).ModifiedValue.bool_value; } }
    private bool Mines { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.orbit_mines).ModifiedValue.bool_value; } }
    private bool Boomerang { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.orbit_boomerang).ModifiedValue.bool_value; } }

    private List<OrbitRing> rings = new List<OrbitRing>();

    private int CalculatedRingCount => Mathf.Max(1, (int)(RingCount * (Split ? 0.5f : 1f)));
    private int CalculatedProjectileCount => (int)(ProjectileCount * (Split ? 2f : 1f));

    public class OrbitRing
    {
        public int ProjectileCount { get; set; }
        public float ProjectileSize { get; set; }
        public float OrbitTime { get; set; }
        public float Radius { get; set; }
        public float AngleOffset { get; set; }
        public bool HasChain { get; set; }
        public bool HasMiniOrbit { get; set; }
        public int? OverrideDirectionMul { get; set; }

        private List<OrbitProjectile> projectiles = new List<OrbitProjectile>();
        private OrbitProjectile prefab_projectile;
        private Transform _target;
        private Vector3 _target_position;
        private float _time;

        public OrbitRing(OrbitProjectile prefab_projectile, Transform target)
        {
            this.prefab_projectile = prefab_projectile;
            _target = target;
        }

        public OrbitRing(OrbitProjectile prefab_projectile, Vector3 position)
        {
            this.prefab_projectile = prefab_projectile;
            _target_position = position;
        }

        public void Clear()
        {
            projectiles
                .Where(x => x != null)
                .ToList()
                .ForEach(x => x.Kill());
            projectiles.Clear();
        }

        public void Update(int i_ring = 0)
        {
            UpdateProjectiles();
            UpdateOrbit(i_ring);
        }

        public void UpdateProjectiles()
        {
            if (projectiles.Count < ProjectileCount)
            {
                var diff = ProjectileCount - projectiles.Count;
                if (diff > 0)
                {
                    for (int i = 0; i < diff; i++)
                    {
                        CreateProjectile(this);
                    }
                }
                else
                {
                    for (int i = 0; i < Mathf.Abs(diff); i++)
                    {
                        var p = projectiles[projectiles.Count - 1];
                        if (p == null) continue;
                        p.Kill();
                        projectiles.Remove(p);
                    }
                }
            }

            foreach (var p in projectiles)
            {
                if (p == null) continue;
                p.transform.localScale = Vector3.one * ProjectileSize;
                p.SetChainEnabled(HasChain);
                p.SetMiniOrbitEnabled(HasMiniOrbit);
            }
        }

        public void UpdateOrbit(int i_ring = 0)
        {
            var dir_mul = OverrideDirectionMul ?? (i_ring % 2 == 0 ? 1 : -1);

            var radius = Radius;
            var angle_delta = 360f / projectiles.Count;

            _time = (_time + Time.deltaTime) % OrbitTime;

            for (int i_proj = 0; i_proj < projectiles.Count; i_proj++)
            {
                var p = projectiles[i_proj];
                if (p == null) continue;

                var angle_start = angle_delta * i_proj;
                var angle_time = Mathf.Lerp(0f, 360f, _time / OrbitTime);
                var angle = AngleOffset + angle_start + angle_time * dir_mul;
                var q_angle = Quaternion.AngleAxis(angle, Vector3.forward);

                var position_angle = q_angle * Vector3.up * radius;
                var position_player = _target?.position ?? _target_position;
                var position = position_angle + position_player;
                p.transform.position = position;

                p.mini_orbit_direction_override = dir_mul * -1;
            }
        }

        private void CreateProjectile(OrbitRing ring)
        {
            var inst = ProjectileController.Instance.CreateProjectile(prefab_projectile);
            inst.Lifetime = -1;
            inst.Piercing = -1;
            inst.Rigidbody.isKinematic = true;
            ring.projectiles.Add(inst);
        }
    }

    public override float GetBaseCooldown() => 1;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
        Player.onDeath += Clear;
    }

    public override Dictionary<string, string> GetStats()
    {
        var stats = base.GetStats();

        stats.Add("Orbits", $"{CalculatedRingCount}");
        stats.Add("Projectiles", $"{CalculatedProjectileCount}");
        stats.Add("Projectile size", $"{ProjectileSize}");
        stats.Add("Orbit time", $"{ProjectileOrbitTime}");

        return stats;
    }

    protected override void Update()
    {
        base.Update();

        if (AbilityController.Instance.GetPrimaryAbility() != this) return;

        UpdateRings();
    }

    public override void DestroyAbility()
    {
        base.DestroyAbility();
        Clear();
    }

    private void Clear()
    {
        rings.ForEach(x => x.Clear());
        rings.Clear();
    }

    private void UpdateRings()
    {
        // Rings
        if (rings.Count != CalculatedRingCount)
        {
            var diff = CalculatedRingCount - rings.Count;
            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    rings.Add(new OrbitRing(prefab_projectile, Player.Instance.transform));
                }
            }
            else
            {
                for (int i = 0; i < Mathf.Abs(diff); i++)
                {
                    var ring = rings[rings.Count - 1];
                    ring.Clear();
                    rings.Remove(ring);
                }
            }
        }

        // Projectiles
        var boomerang_radius = Boomerang ? ((Mathf.Sin(Time.time * 2) + 1) / 2) * 4f : 0;
        var boomerang_time_mul = Boomerang ? 0.75f : 1f;
        for (int i_ring = 0; i_ring < rings.Count; i_ring++)
        {
            var ring = rings[i_ring];
            ring.OrbitTime = ProjectileOrbitTime * (1f + 0.05f * i_ring) * boomerang_time_mul;
            ring.Radius = RingRadius + 2f * i_ring + boomerang_radius;
            ring.ProjectileCount = CalculatedProjectileCount;
            ring.ProjectileSize = ProjectileSize;
            ring.HasChain = Chain;
            ring.HasMiniOrbit = Mines;
            ring.Update(i_ring);
        }
    }
}