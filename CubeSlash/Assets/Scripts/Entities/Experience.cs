using UnityEngine;

public class Experience : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rig;
    [SerializeField] private SpriteRenderer spr;
    [SerializeField] private Color c_plant;
    [SerializeField] private Color c_meat;
    [SerializeField] private AnimationCurve ac_collect;

    private bool IsCollected { get; set; }
    private float TimeCollected { get; set; }

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
            var dir = transform.DirectionTo(Player.Instance.transform.position).normalized;
            var t = (Time.time - TimeCollected) / 0.25f;
            rig.velocity = dir * ac_collect.Evaluate(t) * 10f;

            if(Vector3.Distance(transform.position, Player.Instance.transform.position) <= 0.1f)
            {
                Player.Instance.Experience.Value++;
                Despawn();
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, Player.Instance.transform.position) <= Player.Instance.DistanceCollect)
            {
                IsCollected = true;
                TimeCollected = Time.time;
            }
        }
    }

    private void DespawnUpdate()
    {
        if (Player.Instance == null) return;
        if (Vector3.Distance(transform.position, Player.Instance.transform.position) > CameraController.Instance.Width * 2)
        {
            Despawn();
        }
    }

    public void Despawn()
    {
        ExperienceController.Instance.OnExperienceDespawned(this);
    }

    public void SetMeat()
    {
        spr.color = c_meat;
    }
    
    public void SetPlant()
    {
        spr.color = c_plant;
    }
}