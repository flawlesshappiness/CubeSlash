using System.Collections.Generic;
using UnityEngine;

public class AbilityDashClone : MonoBehaviourExtended
{
    [SerializeField] private List<ParticleSystem> particles = new List<ParticleSystem>();

    public Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(); } }
    public DamageTrail Trail { get { return GetComponentOnce<DamageTrail>(ComponentSearchType.CHILDREN); } }

    public System.Action<IKillable> onHitKillable;

    private AbilityDash Dash { get; set; }
    private bool HasPlayer { get; set; }

    public void Initialize(AbilityDash dash, bool has_player)
    {
        Dash = dash;
        HasPlayer = has_player;

        // Trail
        Trail.gameObject.SetActive(Dash.TrailEnabled);
        Trail.ResetTrail();

        // Camera
        if (has_player)
        {
            CameraController.Instance.Target = transform;
        }
    }

    public void Destroy()
    {
        UpdateTrail();

        foreach(var ps in particles)
        {
            ps.transform.parent = GameController.Instance.world;
            ps.ModifyEmission(e => e.enabled = false);
            Destroy(ps.gameObject, ps.main.startLifetime.constant * 2);
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var k = collision.GetComponentInParent<IKillable>();
        if(k != null)
        {
            onHitKillable?.Invoke(k);
        }
    }

    public void DashUpdate()
    {
        UpdateTrail();
        UpdatePlayer();
    }

    private void UpdateTrail()
    {
        if (Dash.TrailEnabled)
        {
            Trail.UpdateTrail();
        }
    }
    
    private void UpdatePlayer()
    {
        if (HasPlayer)
        {
            Dash.Player.transform.position = transform.position;
        }
    }
}