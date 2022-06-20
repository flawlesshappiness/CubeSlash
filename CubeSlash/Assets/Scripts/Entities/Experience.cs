using UnityEngine;

public class Experience : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spr;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Color c_plant;
    [SerializeField] private Color c_meat;
    [SerializeField] private AnimationCurve ac_collect;

    private bool IsCollected { get; set; }
    private float TimeCollected { get; set; }
    private Vector3 PositionCollected { get; set; }

    public void Initialize()
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
            if (Vector3.Distance(transform.position, Player.Instance.transform.position) <= Player.Instance.DistanceCollect)
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
                Player.Instance.Experience.Value++;
            }

            Despawn();
        }
    }

    public void Despawn()
    {
        ExperienceController.Instance.OnExperienceDespawned(this);
    }

    public void SetMeat()
    {
        SetColor(c_meat);
    }
    
    public void SetPlant()
    {
        SetColor(c_plant);
    }

    private void SetColor(Color c)
    {
        spr.color = c;
        trail.material.color = c;
    }
}