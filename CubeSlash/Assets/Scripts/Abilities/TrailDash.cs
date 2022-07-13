using UnityEngine;

public class TrailDash : MonoBehaviour
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
            var e = hit.GetComponentInParent<Enemy>();
            if(e != null && e.IsKillable())
            {
                e.Kill();
            }
        }
    }

    public void ResetTrail()
    {
        pos_prev = transform.position;
    }

    public void UpdateTrail()
    {
        var pos_next = transform.position;
        var dir = pos_next - pos_prev;
        dir = Vector3.ClampMagnitude(dir, dir.magnitude - radius);
        if(dir.magnitude >= radius * 2)
        {
            pos_prev = pos_prev + dir.normalized * radius * 2;
            CreateTrail(pos_prev);
        }
    }

    private void CreateTrail(Vector3 position)
    {
        var trail = Instantiate(gameObject, GameController.Instance.world).GetComponent<TrailDash>();
        trail.gameObject.SetActive(true);
        trail.transform.position = position;
    }
}