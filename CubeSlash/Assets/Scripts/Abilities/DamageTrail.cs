using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class DamageTrail : MonoBehaviour
{
    public float radius;
    public float lifetime;

    [SerializeField] private AnimationCurve ac_size;

    private Vector3 pos_prev;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void Update()
    {
        HitTargets();
    }

    private void HitTargets()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach(var hit in hits)
        {
            var k = hit.GetComponentInParent<IKillable>();
            if(k != null && k.CanKill())
            {
                k.Kill();
            }
        }
    }

    public void ResetTrail()
    {
        pos_prev = transform.position;
    }

    public void CreateTrailsFromPreviousPosition()
    {
        var dir = (transform.position - pos_prev).normalized * radius;
        var create_more = true;
        while (create_more)
        {
            var pos_next = pos_prev + dir;
            if(Vector3.Distance(pos_next, transform.position) > radius)
            {
                create_more = true;
                CreateTrail(pos_next);
                pos_prev = pos_next;
            }
            else
            {
                create_more = false;
            }
        }
    }

    private void CreateTrail(Vector3 position)
    {
        var trail = Instantiate(gameObject, GameController.Instance.world).GetComponent<DamageTrail>();
        trail.lifetime = lifetime;
        trail.radius = radius;
        trail.transform.position = position;
        trail.gameObject.SetActive(true);
        trail.StopParticleSystems();
        Lerp.LocalScale(trail.transform, lifetime, Vector3.zero, Vector3.one).Curve(ac_size);
        Destroy(trail.gameObject, lifetime);
    }

    public void StopParticleSystems()
    {
        foreach(var ps in GetComponentsInChildren<ParticleSystem>())
        {
            this.StartCoroutineWithID(StopParticleSystemCr(ps), "stop_" + ps.GetInstanceID());
        }
    }

    private IEnumerator StopParticleSystemCr(ParticleSystem ps)
    {
        var delay = Mathf.Clamp(lifetime - ps.main.startLifetime.constant, 0.5f, lifetime);
        yield return new WaitForSeconds(delay);
        ps.transform.parent = GameController.Instance.world;
        ps.SetEmissionEnabled(false);
    }
}