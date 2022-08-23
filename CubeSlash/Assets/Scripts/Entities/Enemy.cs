using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviourExtended, IKillable
{
    public Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.CHILDREN); } }
    public Character Character { get; private set; }
    private EntityAI AI { get; set; }
    public EnemySettings Settings { get; private set; }
    public Vector3 MoveDirection { get { return Character.transform.up; } }
    public MultiLock MovementLock { get; private set; } = new MultiLock();
    public MultiLock DragLock { get; private set; } = new MultiLock();

    public float LinearAcceleration { get; set; }
    public float LinearVelocity { get; set; }
    public float AngularAcceleration { get; set; }
    public float AngularVelocity { get; set; }

    public event System.Action OnDeath;

    public void Initialize(EnemySettings settings)
    {
        this.Settings = settings;
        transform.localScale = Vector3.one * settings.size;
        Rigidbody.mass = settings.mass;
        SetCharacter(settings.character);
        SetAI(settings.ai);

        OnDeath = null;
    }

    private void Update()
    {
        DragUpdate();
    }

    private void SetCharacter(Character prefab)
    {
        if (Character)
        {
            Destroy(Character.gameObject);
            Character = null;
        }

        Character = Instantiate(prefab.gameObject, transform).GetComponent<Character>();
        Character.Initialize();
    }

    public void Reposition()
    {
        transform.position = CameraController.Instance.GetPositionOutsideCamera();
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

    #region MOVEMENT
    private void DragUpdate()
    {
        Rigidbody.velocity = Vector3.ClampMagnitude(Rigidbody.velocity, LinearVelocity);
        Rigidbody.angularVelocity = Mathf.Clamp(Rigidbody.angularVelocity, -AngularVelocity, AngularVelocity);
    }

    public void Move(Vector3 direction)
    {
        if (MovementLock.IsFree)
        {
            Rigidbody.AddForce(direction.normalized * LinearAcceleration * Rigidbody.mass);
        }
    }

    public void Turn(bool right)
    {
        var angle = right ? -1 : 1;
        Rigidbody.AddTorque(angle * AngularAcceleration * Rigidbody.mass);
    }
    #endregion
    #region HEALTH
    public bool CanKill() => !Character.HasActiveHealthDuds();

    public void Kill()
    {
        Character.ps_death.Duplicate()
            .Position(transform.position)
            .Scale(Vector3.one * Settings.size)
            .Destroy(10)
            .Play();

        // Event
        OnDeath?.Invoke();

        // AI
        SetAI(null);

        // Respawn
        Respawn();
    }

    public void Respawn()
    {
        gameObject.SetActive(false);
        EnemyController.Instance.EnemyKilled(this);
    }
    #endregion
}
