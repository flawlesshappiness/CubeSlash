using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : Character, IKillable
{
    private EntityAI AI { get; set; }
    public EnemySettings Settings { get; private set; }
    public Vector3 MoveDirection { get { return Body.transform.up; } }

    public event System.Action OnDeath;

    public void Initialize(EnemySettings settings)
    {
        this.Settings = settings;
        transform.localScale = Vector3.one * settings.size;
        Rigidbody.mass = settings.mass;
        SetBody(settings.body);
        SetAI(settings.ai);

        OnDeath = null;
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

    public void Turn(bool right)
    {
        var angle = right ? -1 : 1;
        Rigidbody.AddTorque(angle * AngularAcceleration * Rigidbody.mass);
    }
    #region HEALTH
    public bool CanKill() => !Body.HasActiveHealthDuds();

    public void Kill()
    {
        Body.ps_death.Duplicate()
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
