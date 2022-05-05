using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Projectile : MonoBehaviourExtended
{
    public float TurnSpeed { get; set; }
    public bool Homing { get; set; }
    public Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.THIS); } }
    public System.Action<Enemy> OnHitEnemy { get; set; }
    public System.Action<Projectile> OnHitProjectile { get; set; }
    public Enemy Target { get; set; }

    private void Start()
    {
        StartCoroutine(FindTargetCr());
    }

    private void OnDrawGizmos()
    {
        if (!Target) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, Target.transform.position);
    }

    private void FixedUpdate()
    {
        TurnTowardsTarget();
    }

    private void TurnTowardsTarget()
    {
        if (Target == null) return;
        if (!Homing) return;
        var dir = Target.transform.position - transform.position;
        var angle = Vector3.SignedAngle(transform.up, dir, Vector3.back);
        var sign = -1 * Mathf.Sign(angle);

        if(angle.Abs() > 90)
        {
            // Don't even try
        }
        else if(angle.Abs() < TurnSpeed)
        {
            SetDirection(dir);
        }
        else
        {
            Turn(TurnSpeed * sign);
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

    public void Destroy(float time)
    {
        StartCoroutine(DestroyCr(time));
    }

    private IEnumerator DestroyCr(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy();
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var enemy = collision.gameObject.GetComponentInParent<Enemy>();
        if (enemy)
        {
            OnHitEnemy?.Invoke(enemy);
            return;
        }

        var projectile = collision.gameObject.GetComponentInParent<Projectile>();
        if (projectile)
        {
            OnHitProjectile?.Invoke(projectile);
            return;
        }
    }

    private class FindTargetMap
    {
        public Enemy Enemy { get; set; }
        public float Value { get; set; }
    }

    private IEnumerator FindTargetCr()
    {
        while(Target == null)
        {
            FindTarget();
            yield return new WaitForSeconds(1f);
        }
    }

    private void FindTarget()
    {
        var dist_max = 20;
        var forward = transform.up;
        var targets = new List<FindTargetMap>();
        var hits = Physics2D.OverlapCircleAll(transform.position, dist_max);
        foreach(var hit in hits)
        {
            var e = hit.gameObject.GetComponentInParent<Enemy>();
            if (e == null) continue;

            var target = new FindTargetMap();
            targets.Add(target);

            target.Enemy = e;
            var dir = e.transform.position - transform.position;
            var angle = Vector3.Angle(forward, dir.normalized);
            var v_angle = 1f - (angle / 180f);

            var dist = dir.magnitude;
            var v_dist = (1f - (dist / dist_max)) * 1.5f;

            target.Value = v_angle + v_dist;
        }

        targets = targets.OrderByDescending(target => target.Value).ToList();
        Target = targets.Count == 0 ? null : targets[0].Enemy;
    }
}
