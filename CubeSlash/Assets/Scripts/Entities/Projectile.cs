using UnityEngine;

public class Projectile : MonoBehaviourExtended
{
    [SerializeField] private bool hits_player, hits_enemy;
    [SerializeField] protected Transform pivot_animation;
    [SerializeField] private ParticleSystem ps_death, ps_trail;

    public float Drag { get; set; } = 1f;
    public float Lifetime { get; set; } = 1f;
    public int Piercing { get; set; }
    public Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.THIS); } }
    public CircleCollider2D Collider { get { return GetComponentOnce<CircleCollider2D>(ComponentSearchType.CHILDREN); } }

    public System.Action<Player> onHitPlayer;
    public System.Action<IKillable> onHitEnemy;
    public System.Action onDeath;
    public System.Action onDestroy;

    public float BirthTime { get; private set; }
    public float DeathTime { get; private set; }

    protected virtual void Start()
    {
        BirthTime = Time.time;
    }

    public virtual void Update()
    {
        LifetimeUpdate();
        DistanceUpdate();
    }

    private void FixedUpdate()
    {
        DragUpdate();
    }

    private void LifetimeUpdate()
    {
        if (Lifetime < 0) return;
        var t = (Time.time - BirthTime) / Lifetime;
        if (t >= 1)
        {
            onDeath?.Invoke();
            Kill();
        }
    }

    private void DistanceUpdate()
    {
        var dist = CameraController.Instance.Width * 2;
        if (Vector3.Distance(transform.position, Player.Instance.transform.position) > dist)
        {
            Kill();
        }
    }
    private void DragUpdate()
    {
        if (Rigidbody.velocity.magnitude > 0)
        {
            Rigidbody.velocity *= Drag;
        }
    }

    public void SetDirection(Vector3 direction)
    {
        var angle = Vector3.SignedAngle(Vector3.up, direction, Vector3.back);
        var q = Quaternion.AngleAxis(angle, Vector3.back);
        transform.rotation = q;
    }

    public void Kill()
    {
        if (ps_death != null)
        {
            ps_death.Duplicate()
                .Parent(GameController.Instance.world)
                .Play()
                .Destroy(1);
        }

        if (ps_trail != null)
        {
            ps_trail.transform.parent = null;
            Destroy(ps_trail.gameObject, 2);
        }

        onDestroy?.Invoke();

        gameObject?.SetActive(false);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var hit_success = false;
        if (hits_player)
        {
            var player = collision.GetComponentInParent<Player>();
            if (player != null)
            {
                player.Damage(transform.position);
                onHitPlayer?.Invoke(player);
                hit_success = true;
            }
        }
        else if (hits_enemy)
        {
            var k = collision.GetComponentInParent<IKillable>();
            if (k != null)
            {
                onHitEnemy?.Invoke(k);

                if (Player.Instance.TryKillEnemy(k))
                {
                    hit_success = true;
                }
            }
        }

        if (hit_success)
        {
            var can_pierce = Piercing != 0;
            if (can_pierce)
            {
                Piercing = Mathf.Clamp(Piercing - 1, -1, int.MaxValue);
            }
            else
            {
                Kill();
            }
        }
    }
}
