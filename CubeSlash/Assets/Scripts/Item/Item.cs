using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("ITEM")]
    [SerializeField] private AnimationCurve ac_collect;
    public float mul_dist_collect = 1f;
    public bool IsBeingCollected { get; set; }
    public bool IsDespawning { get; set; }

    private const float DURATION_PER_UNIT = 0.03f;

    public virtual void Initialize()
    {
        IsBeingCollected = false;
        IsDespawning = false;
    }

    private void Update()
    {
        DespawnUpdate();
    }

    private void FixedUpdate()
    {
        CollectUpdate();
    }

    protected virtual void Collect()
    {
        IsBeingCollected = false;
    }

    private void CollectUpdate()
    {
        if (Player.Instance == null) return;
        if (IsDespawning) return;
        if (IsBeingCollected) return;

        var collect_distance = 3f * mul_dist_collect;
        var can_collect = Vector3.Distance(transform.position, Player.Instance.transform.position) <= collect_distance;
        if (can_collect)
        {
            IsBeingCollected = true;
            AnimateCollect();
        }
    }

    public CustomCoroutine AnimateCollect()
    {
        IsBeingCollected = true;
        return this.StartCoroutineWithID(CollectCr(), "Collect_" + GetInstanceID());
    }

    private IEnumerator CollectCr()
    {
        var start = transform.position;
        yield return LerpEnumerator.Value(0.1f, f =>
        {
            var t = EasingCurves.EaseOutQuad.Evaluate(f);
            var dir = (start - Player.Instance.transform.position).normalized;
            var end = start + dir * 2f;
            transform.position = Vector3.Lerp(start, end, t);
        }).Connect(gameObject);

        start = transform.position;
        var distance = Vector3.Distance(start, Player.Instance.transform.position);
        var duration = DURATION_PER_UNIT * distance;
        yield return LerpEnumerator.Value(duration, f =>
        {
            var t = EasingCurves.EaseInQuad.Evaluate(f);
            var end = Player.Instance.transform.position;
            transform.position = Vector3.Lerp(start, end, t);
        }).Connect(gameObject);

        if (!Player.Instance.IsDead)
        {
            Collect();
        }

        Despawn();
    }

    private void DespawnUpdate()
    {
        if (Player.Instance == null) return;
        var too_far_away = Vector3.Distance(transform.position, Player.Instance.transform.position) > CameraController.Instance.Width * 2;
        var should_despawn = too_far_away;
        if (should_despawn)
        {
            Despawn();
        }
    }

    public virtual void Despawn()
    {
        IsDespawning = true;
    }
}