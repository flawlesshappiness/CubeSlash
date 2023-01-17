using EasingCurve;
using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("ITEM")]
    [SerializeField] private AnimationCurve ac_collect;
    public float duration_collect = 0.25f;
    public float mul_dist_collect = 1f;
    public bool IsBeingCollected { get; set; }
    public bool IsDespawning { get; set; }

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

        var collect_distance = Player.Instance.CollectRadius * mul_dist_collect;
        var can_collect = Vector3.Distance(transform.position, Player.Instance.transform.position) <= collect_distance;
        if (can_collect)
        {
            IsBeingCollected = true;
            this.StartCoroutineWithID(Cr(), "Collect_" + GetInstanceID());
        }

        IEnumerator Cr()
        {
            var start = transform.position;
            var curve = ac_collect;
            yield return LerpEnumerator.Value(duration_collect, f =>
            {
                var t = curve.Evaluate(f);
                transform.position = Vector3.LerpUnclamped(start, Player.Instance.transform.position, t);
            }).Connect(gameObject);

            Collect();
            Despawn();
        }
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