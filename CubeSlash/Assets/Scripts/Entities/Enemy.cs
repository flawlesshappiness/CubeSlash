using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviourExtended
{
    [Min(1)] public int health;
    public bool IsBoss { get; set; }

    private Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.CHILDREN); } }
    private Character Character { get; set; }
    private EntityAI AI { get; set; }

    private bool Moving { get; set; }

    private const float SPEED_MOVE = 3f;

    private event System.Action onDeath;

    public void Initialize(EnemySettings settings)
    {
        health = settings.health;
        IsBoss = settings.boss;
        SetCharacter(settings.character);
        SetAI(settings.ai);
        transform.localScale = settings.size;
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
        Moving = true;
        Rigidbody.velocity = direction.normalized * SPEED_MOVE;
        Character.SetLookDirection(direction);
    }

    private void DecelerateUpdate()
    {
        if (Moving)
        {
            Moving = false;
        }
        else
        {
            // Decelerate
            Rigidbody.velocity = Rigidbody.velocity * 0.7f;
        }
    }

    public void Damage(int amount)
    {
        health -= amount.Abs();

        if(health <= 0)
        {
            Kill();
        }
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
        onDeath?.Invoke();

        // Respawn
        Respawn();
    }

    public void Respawn()
    {
        gameObject.SetActive(false);
        EnemyController.Instance.EnemyKilled(this);
    }
}
