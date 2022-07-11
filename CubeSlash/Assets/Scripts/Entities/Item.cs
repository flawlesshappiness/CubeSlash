using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("ITEM")]
    [SerializeField] private AnimationCurve ac_collect;
    public float mul_dist_collect = 1f;
    private bool IsCollected { get; set; }
    private float TimeCollected { get; set; }
    private Vector3 PositionCollected { get; set; }

    public virtual void Initialize()
    {
        IsCollected = false;
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

    }

    private void CollectUpdate()
    {
        if (Player.Instance == null) return;

        if (IsCollected)
        {
            var t = (Time.time - TimeCollected) / 0.25f;
            transform.position = Vector3.LerpUnclamped(PositionCollected, Player.Instance.transform.position, ac_collect.Evaluate(t));
        }
        else
        {
            if (Vector3.Distance(transform.position, Player.Instance.transform.position) <= Player.Instance.DistanceCollect * mul_dist_collect)
            {
                IsCollected = true;
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
            if (in_collect_distance)
            {
                Collect();
            }

            Despawn();
        }
    }

    public virtual void Despawn()
    {
        
    }
}