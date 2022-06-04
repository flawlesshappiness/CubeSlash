using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviourExtended
{
    private Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.CHILDREN); } }
    private Character Character { get; set; }
    private EntityAI AI { get; set; }
    public bool IsParasite { get; private set; }

    public MultiLock MovementLock { get; private set; } = new MultiLock();
    public MultiLock DragLock { get; private set; } = new MultiLock();

    private bool Moving { get; set; }

    private const float SPEED_MOVE = 3f;

    public event System.Action OnDeath;

    public void Initialize(EnemySettings settings)
    {
        SetCharacter(settings.character);
        SetAI(settings.ai);
        transform.localScale = settings.size;

        // Parasites
        if(settings.parasite != null)
        {
            foreach (var space in Character.ParasiteSpaces.Where(space => space.Empty))
            {
                var e = EnemyController.Instance.CreateEnemy();
                e.Initialize(settings.parasite);
                space.SetParasite(e);
            }
        }
    }

    private void Update()
    {
        if (!Character) return;

        DecelerateUpdate();
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

    public void SetParasite(bool isParasite)
    {
        IsParasite = isParasite;
        Rigidbody.isKinematic = isParasite;
        Character.Collider.enabled = isParasite;
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

    public void Move(Vector3 direction)
    {
        if (MovementLock.IsFree)
        {
            Moving = true;
            Rigidbody.velocity = direction.normalized * SPEED_MOVE;
            Character.SetLookDirection(direction);
        }
    }

    public void Decelerate(float mul)
    {
        if (DragLock.IsFree)
        {
            Rigidbody.velocity = Rigidbody.velocity * mul;
        }
    }

    private void DecelerateUpdate()
    {
        if (Moving)
        {
            Moving = false;
        }
        else
        {
            Decelerate(0.7f);
        }
    }

    public bool IsKillable()
    {
        return Character.ParasiteSpaces.Count == 0 || Character.ParasiteSpaces.Count(space => !space.Empty) == 0;
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

        // Respawn
        Respawn();
    }

    public void Respawn()
    {
        gameObject.SetActive(false);
        EnemyController.Instance.EnemyKilled(this);
    }
}
