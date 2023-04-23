using UnityEngine;

public class SpawnPlant : SpawnObject, IKillable
{
    public int exp_min;
    public int exp_max;
    public float spawn_radius;
    public float chance_health;
    public HealthPoint.Type type_health;

    [SerializeField] private Rigidbody2D Rigidbody;
    [SerializeField] private ParticleSystem ps_death;
    [SerializeField] private SoundEffectType type_sfx_death;

    public bool IsDead { get; private set; }

    public bool CanHit() => !IsDead;
    public bool CanKill() => CanHit();
    public Vector3 GetPosition() => transform.position;

    public bool TryKill()
    {
        if (!CanKill()) return false;

        SpawnExp();
        SpawnHealth();

        ps_death.Duplicate()
            .Position(transform.position)
            .Scale(ps_death.transform.lossyScale)
            .Parent(GameController.Instance.world)
            .Play()
            .Destroy(ps_death.main.startLifetime.constant * 2f);

        SoundController.Instance.Play(type_sfx_death);

        IsDead = true;
        Destroy();

        return true;
    }

    public override void Initialize()
    {
        base.Initialize();

        IsDead = false;

        var dir_to_player = Player.Instance.transform.position - transform.position;
        var force = Random.Range(10f, 100f);
        Rigidbody.AddForce(Rigidbody.mass * dir_to_player.normalized * force);

        var torque = Random.Range(-100f, 100f);
        Rigidbody.AddTorque(Rigidbody.mass * torque);
    }

    private void SpawnExp()
    {
        var count = Random.Range(exp_min, exp_max + 1);
        for (int i = 0; i < count; i++)
        {
            var position = transform.position + Random.insideUnitCircle.ToVector3() * spawn_radius;
            var exp = ItemController.Instance.SpawnPlant(position);
            exp.AnimateCollect();
        }
    }

    private void SpawnHealth()
    {
        var chance = Random.Range(0f, 1f);
        if(chance < chance_health)
        {
            if(type_health == HealthPoint.Type.TEMPORARY)
            {
                ItemController.Instance.SpawnArmor(transform.position);
            }
            else if(type_health == HealthPoint.Type.FULL)
            {
                ItemController.Instance.SpawnHealth(transform.position);
            }
        }
    }
}