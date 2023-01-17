using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : Character, IKillable, IHurt
{
    [SerializeField] private FMODEventReference event_death;
    private EnemyAI AI { get; set; }
    public EnemySettings Settings { get; private set; }
    public Vector3 MoveDirection { get { return Body.transform.up; } }
    public EnemyBody EnemyBody { get { return Body as EnemyBody; } }

    public bool IsDead { get; private set; }
    public MultiLock InvincibleLock { get; private set; } = new MultiLock();

    public event System.Action OnDeath;

    public void Initialize(EnemySettings settings)
    {
        IsDead = false;
        OnDeath = null;

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
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        RepositionUpdate();
    }

    public Vector3 GetPosition() => transform.position;

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

    private void SetAI(EnemyAI prefab)
    {
        if (AI)
        {
            AI.Kill();
            AI = null;
        }

        if (prefab)
        {
            AI = Instantiate(prefab.gameObject, transform).GetComponent<EnemyAI>();
            AI.Initialize(this);
        }
    }

    public void Turn(bool right)
    {
        var angle = right ? -1 : 1;
        Rigidbody.AddTorque(angle * AngularAcceleration * Rigidbody.mass);
    }
    #region HEALTH
    public bool CanKill() => !EnemyBody.HasActiveHealthDuds() && !IsDead && InvincibleLock.IsFree;

    public void Kill()
    {
        if (!IsDead)
        {
            IsDead = true;

            // PS
            if(Body.ps_death != null)
            {
                Body.ps_death.Duplicate()
                    .Position(transform.position)
                    .Scale(Vector3.one * Settings.size)
                    .Destroy(10)
                    .Play();
            }

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

    public void SetInvincible(string id, float duration)
    {
        this.StartCoroutineWithID(Cr(), "invincible_" + GetInstanceID());
        IEnumerator Cr()
        {
            InvincibleLock.AddLock(id);
            yield return new WaitForSeconds(duration);
            InvincibleLock.RemoveLock(id);
        }
    }
    #endregion
    #region IHURT
    public bool CanHurt() => true;
    #endregion
}
