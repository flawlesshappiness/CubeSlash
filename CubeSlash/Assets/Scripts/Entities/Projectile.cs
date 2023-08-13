using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviourExtended
{
    [SerializeField] private bool hits_player, hits_enemy;
    [SerializeField] private Transform pivot_animation;
    [SerializeField] private ParticleSystem ps_death, ps_trail;
    [SerializeField] private bool scale_from_zero;
    [SerializeField] private float anim_scale_duration;
    [SerializeField] private float anim_rotation;
    [SerializeField] private AnimationCurve curve_scale;
    public float Drag { get; set; } = 1f;
    public float Lifetime { get; set; } = 1f;
    public int Piercing { get; set; }
    public Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.THIS); } }
    public System.Action<Player> onHitPlayer;
    public System.Action<IKillable> onHitEnemy;
    public System.Action onDeath;
    public System.Action onDestroy;

    public float BirthTime { get; private set; }
    public float DeathTime { get; private set; }

    protected virtual void Start()
    {
        BirthTime = Time.time;
        DeathTime = BirthTime + Lifetime;

        if (scale_from_zero)
        {
            StartCoroutine(AnimateScaleBeginCr());
        }
        else
        {
            StartCoroutine(AnimateScaleCr());
        }
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
        var t = (Time.time - BirthTime) / (DeathTime - BirthTime);
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

        gameObject.SetActive(false);
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
                hit_success = true;
                onHitEnemy?.Invoke(k);

                if (Player.Instance.TryKillEnemy(k))
                {

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

    private IEnumerator AnimateScaleBeginCr()
    {
        var end = Vector3.one * curve_scale.Evaluate(0);
        yield return LerpEnumerator.LocalScale(pivot_animation, 0.2f, Vector3.zero, end);
        StartCoroutine(AnimateScaleCr());
    }

    private IEnumerator AnimateScaleCr()
    {
        while (true)
        {
            yield return LerpEnumerator.Value(anim_scale_duration, f =>
            {
                var t = curve_scale.Evaluate(f);
                pivot_animation.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, t);
            });
        }
    }

    public Coroutine AnimateRotation()
    {
        return StartCoroutine(AnimateRotationCr());
    }

    private IEnumerator AnimateRotationCr()
    {
        var angle = Random.Range(-anim_rotation, anim_rotation);
        while (true)
        {
            var z = (pivot_animation.eulerAngles.z + angle * Time.deltaTime) % 360f;
            pivot_animation.eulerAngles = pivot_animation.eulerAngles.SetZ(z);
            yield return null;
        }
    }
}
