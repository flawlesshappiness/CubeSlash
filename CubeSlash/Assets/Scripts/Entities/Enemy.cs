using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviourExtended
{
    public Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.CHILDREN); } }
    public Character Character { get; private set; }
    private EntityAI AI { get; set; }
    public EnemySettings Settings { get; private set; }
    public bool IsParasite { get { return ParasiteHost != null; } }
    public Enemy ParasiteHost { get; private set; }
    public Vector3 MoveDirection { get { return Character.transform.up; } }
    public MultiLock MovementLock { get; private set; } = new MultiLock();
    public MultiLock DragLock { get; private set; } = new MultiLock();

    public float Acceleration { get; set; }
    public float SpeedMax { get; set; }

    public event System.Action OnDeath;

    public void Initialize(EnemySettings settings)
    {
        this.Settings = settings;
        SetCharacter(settings.character);
        SetAI(settings.ai);
        transform.localScale = settings.size;

        // Parasites
        if(settings.parasite != null)
        {
            foreach (var space in Character.ParasiteSpaces.Where(space => space.Available))
            {
                var e = EnemyController.Instance.SpawnEnemy(settings.parasite, space.Position);
                space.SetParasite(e);
            }
        }
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

    public void SetParasiteHost(Enemy host)
    {
        ParasiteHost = host;
        Rigidbody.isKinematic = IsParasite;
        Character.Collider.enabled = !IsParasite;
        AI.enabled = !IsParasite;

        Character.SetLookDirection(host.transform.position - transform.position);
    }

    public void RemoveParasiteHost()
    {
        SetParasiteHost(null);
    }

    private void SetAI(EntityAI prefab)
    {
        if (AI)
        {
            Destroy(AI.gameObject);
            AI = null;
        }

        AI = Instantiate(prefab.gameObject, transform).GetComponent<EntityAI>();
        AI.Initialize(this);
    }

    #region MOVEMENT
    private void DragUpdate()
    {
        if(Rigidbody.velocity.magnitude > SpeedMax)
        {
            Rigidbody.velocity *= 0.9f;
        }
    }

    public void Move(Vector3 direction)
    {
        if (MovementLock.IsFree)
        {
            Rigidbody.AddForce(direction.normalized * Acceleration);
        }
    }

    public void Decelerate(float mul)
    {
        if (DragLock.IsFree)
        {
            Rigidbody.velocity = Rigidbody.velocity * mul;
        }
    }
    #endregion
    #region HEALTH
    public bool IsKillable()
    {
        return Character.ParasiteSpaces.Count == 0 || Character.ParasiteSpaces.Count(space => !space.Available) == 0;
    }

    public void Kill()
    {
        InstantiateParticle("Particles/ps_burst")
            .Position(transform.position)
            .Destroy(1)
            .Play();

        InstantiateParticle("Particles/ps_flash")
            .Position(transform.position)
            .Scale(Character.transform.localScale * 5)
            .Destroy(1)
            .Play();

        // Event
        OnDeath?.Invoke();

        // Experience
        var experience = ExperienceController.Instance.SpawnExperience(transform.position);
        experience.Initialize();
        experience.SetMeat();

        // AI
        AI.Kill();
        AI = null;

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
