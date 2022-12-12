using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : Character, IKillable
{
    [SerializeField] private FMODEventReference event_death;
    private EntityAI AI { get; set; }
    public EnemySettings Settings { get; private set; }
    public Vector3 MoveDirection { get { return Body.transform.up; } }
    public EnemyBody EnemyBody { get { return Body as EnemyBody; } }

    public bool IsDead { get; private set; }

    public event System.Action OnDeath;

    public void Initialize(EnemySettings settings)
    {
        this.Settings = settings;
        LinearAcceleration = settings.linear_acceleration;
        LinearVelocity = settings.linear_velocity;
        LinearDrag = settings.linear_drag;
        AngularAcceleration = settings.angular_acceleration;
        AngularVelocity = settings.angular_velocity;

        transform.localScale = Vector3.one * settings.size;
        Rigidbody.mass = settings.mass;
        SetBody(settings.body);
        SetAI(settings.ai);

        IsDead = false;
        OnDeath = null;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        RepositionUpdate();
    }

    public void RepositionUpdate()
    {
        if (Player.Instance == null) return;

        var size = Level.Current.size;
        var sh = size * 0.5f;
        var center = Player.Instance.transform.position;
        var pos = transform.position;

        if (pos.x < center.x - sh) transform.position += new Vector3(size, 0);
        if (pos.x > center.x + sh) transform.position -= new Vector3(size, 0);
        if (pos.y < center.y - sh) transform.position += new Vector3(0, size);
        if (pos.y > center.y + sh) transform.position -= new Vector3(0, size);
    }

    private void SetAI(EntityAI prefab)
    {
        if (AI)
        {
            AI.Kill();
            AI = null;
        }

        if (prefab)
        {
            AI = Instantiate(prefab.gameObject, transform).GetComponent<EntityAI>();
            AI.Initialize(this);
        }
    }

    public void Turn(bool right)
    {
        var angle = right ? -1 : 1;
        Rigidbody.AddTorque(angle * AngularAcceleration * Rigidbody.mass);
    }
    #region HEALTH
    public bool CanKill() => !EnemyBody.HasActiveHealthDuds() && !IsDead;

    public void Kill()
    {
        if (!IsDead)
        {
            IsDead = true;
            Body.ps_death.Duplicate()
            .Position(transform.position)
            .Scale(Vector3.one * Settings.size)
            .Destroy(10)
            .Play();

            FMODController.Instance.PlayWithLimitDelay(event_death);

            // Event
            OnDeath?.Invoke();

            // AI
            SetAI(null);

            // Respawn
            Respawn();
        }
    }

    public void Respawn()
    {
        gameObject.SetActive(false);
        EnemyController.Instance.EnemyKilled(this);
    }

    public Vector3 GetPosition() => transform.position;
    #endregion
}
