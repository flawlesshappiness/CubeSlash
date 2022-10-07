using UnityEngine;

public class DamageTrail : MonoBehaviour
{
    public float radius;
    public float lifetime;

    [SerializeField] private AnimationCurve ac_size;

    private float time_start;
    private Vector3 pos_prev;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void OnEnable()
    {
        time_start = Time.time;
    }

    private void Update()
    {
        HitTargets();
        LifetimeUpdate();
    }

    private void LifetimeUpdate()
    {
        var t = (Time.time - time_start) / lifetime;
        var ts = ac_size.Evaluate(t);
        transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, ts);

        if(Time.time >= time_start + lifetime)
        {
            Destroy(gameObject);
        }
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

    public void UpdateTrail()
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
        trail.gameObject.SetActive(true);
        trail.transform.position = position;
    }
}