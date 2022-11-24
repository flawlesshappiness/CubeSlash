using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("ITEM")]
    [SerializeField] private AnimationCurve ac_collect;
    public float mul_dist_collect = 1f;
    public bool IsBeingCollected { get; set; }
    public bool IsDespawning { get; set; }
    private float TimeCollected { get; set; }
    private Vector3 PositionCollected { get; set; }

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

        if (IsBeingCollected)
        {
            var t = (Time.time - TimeCollected) / 0.25f;
            transform.position = Vector3.LerpUnclamped(PositionCollected, Player.Instance.transform.position, ac_collect.Evaluate(t));
        }
        else
        {
            if (Vector3.Distance(transform.position, Player.Instance.transform.position) <= Player.Instance.CollectRadius * mul_dist_collect)
            {
                IsBeingCollected = true;
                TimeCollected = Time.time;
                PositionCollected = transform.position;
            }
        }
    }

    private void DespawnUpdate()
    {
        if (Player.Instance == null) return;
        var too_far_away = Vector3.Distance(transform.position, Player.Instance.transform.position) > CameraController.Instance.Width * 2;
        var in_collect_distance = Vector3.Distance(transform.position, Player.Instance.transform.position) <= 0.1f;

        var should_despawn = too_far_away || in_collect_distance;
        if (should_despawn)
        {
            if (in_collect_distance && !IsDespawning)
            {
                Collect();
            }

            Despawn();
        }
    }

    public virtual void Despawn()
    {
        IsDespawning = true;
    }
}