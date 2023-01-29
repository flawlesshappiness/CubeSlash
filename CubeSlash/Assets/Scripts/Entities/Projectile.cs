using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Projectile : MonoBehaviourExtended
{
    [SerializeField] private bool hits_player, hits_enemy;
    [SerializeField] private Transform pivot_animation;
    [SerializeField] private ParticleSystem ps_death, ps_trail;
    [SerializeField] private float anim_scale_duration;
    [SerializeField] private float anim_rotation;
    [SerializeField] private AnimationCurve curve_scale;
    public float TurnSpeed { get; set; }
    public float Drag { get; set; } = 1f;
    public float SearchRadius { get; set; } = 20f;
    public float Lifetime { get; set; } = 1f;
    public bool Piercing { get; set; }
    public bool Homing { get; set; }
    public bool SearchForTarget { get; set; } = true;
    public Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.THIS); } }
    public System.Action<Collider2D> onHit;
    public System.Action onDeath;
    public Enemy Target { get; set; }

    private const float ANGLE_MAX = 90;

    private float time_birth;
    private bool searching;

    private void Start()
    {
        time_birth = Time.time;
        StartFindTarget();
        OnStart();
    }

    protected virtual void OnStart() 
    {
        StartCoroutine(AnimateScaleCr());
        StartCoroutine(AnimateRotationCr());
    }

    private void Update()
    {
        LifetimeUpdate();
        DistanceUpdate();
        OnUpdate();
    }

    protected virtual void OnUpdate() { }

    private void FixedUpdate()
    {
        if(Target == null)
        {
            StartFindTarget();
        }

        TurnTowardsTarget();
        DragUpdate();
    }

    private void LifetimeUpdate()
    {
        var t = (Time.time - time_birth) / Lifetime;
        if(t >= 1)
        {
            onDeath?.Invoke();
            Kill();
        }
    }

    private void DistanceUpdate()
    {
        var dist = CameraController.Instance.Width * 2;
        if(Vector3.Distance(transform.position, Player.Instance.transform.position) > dist)
        {
            Kill();
        }
    }

    private void TurnTowardsTarget()
    {
        if (!Homing) return;
        if (Target == null) return;

        var dir = Target.transform.position - transform.position;
        var angle = Vector3.SignedAngle(transform.up, dir, Vector3.back);
        var sign = -1 * Mathf.Sign(angle);

        if(angle.Abs() > ANGLE_MAX)
        {
            // Find a new target
            StartFindTarget();
        }
        else if(angle.Abs() < TurnSpeed)
        {
            SetDirection(dir);
            Rigidbody.velocity = transform.up * Rigidbody.velocity.magnitude;
        }
        else
        {
            Turn(TurnSpeed * sign);
        }
    }

    private void DragUpdate()
    {
        if(Rigidbody.velocity.magnitude > 0)
        {
            Rigidbody.velocity *= Drag;
        }
    }

    private void Turn(float angle)
    {
        var euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0, 0, euler.z + angle);
        Rigidbody.velocity = transform.up * Rigidbody.velocity.magnitude;
    }

    public void SetDirection(Vector3 direction)
    {
        var angle = Vector3.SignedAngle(Vector3.up, direction, Vector3.back);
        var q = Quaternion.AngleAxis(angle, Vector3.back);
        transform.rotation = q;
    }

    public void Kill()
    {
        if(ps_death != null)
        {
            ps_death.Duplicate()
                .Position(transform.position)
                .Play()
                .Destroy(1);
        }

        if(ps_trail != null)
        {
            ps_trail.transform.parent = null;
            Destroy(ps_trail.gameObject, 2);
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        onHit?.Invoke(collision);
    }

    private class FindTargetMap
    {
        public Enemy Enemy { get; set; }
        public float Value { get; set; }
    }

    private void StartFindTarget()
    {
        if (!SearchForTarget) return;
        if (searching) return;
        CoroutineController.Instance.Run(FindTargetCr(), this, "find_target_" + GetInstanceID());
    }

    private IEnumerator FindTargetCr()
    {
        searching = true;
        while(Target == null)
        {
            FindTarget();
            yield return new WaitForSeconds(0.1f);
        }
        searching = false;
    }

    private void FindTarget()
    {
        var dist_max = SearchRadius;
        var targets = new List<FindTargetMap>();
        var hits = Physics2D.OverlapCircleAll(transform.position + transform.forward * dist_max  * 0.5f, dist_max);
        foreach(var hit in hits)
        {
            var e = hit.gameObject.GetComponentInParent<Enemy>();
            if (e == null) continue;
            if (!e.CanKill()) continue;

            var dir = e.transform.position - transform.position;
            var angle = Vector3.Angle(transform.up, dir.normalized);
            var v_angle = 1f - (angle / 180f);
            if (angle > ANGLE_MAX) continue;

            var dist = dir.magnitude;
            var v_dist = (1f - (dist / dist_max)) * 1.5f;

            var target = new FindTargetMap();
            targets.Add(target);
            target.Enemy = e;
            target.Value = v_angle + v_dist;
        }

        targets = targets.OrderByDescending(target => target.Value).ToList();
        Target = targets.Count == 0 ? null : targets[0].Enemy;
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
