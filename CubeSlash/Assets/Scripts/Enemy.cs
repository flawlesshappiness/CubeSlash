using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviourExtended, IInitializable
{
    private Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.CHILDREN); } }
    private Character Character { get { return GetComponentOnce<Character>(ComponentSearchType.CHILDREN); } }

    public bool Moving { get; private set; }
    private Vector3 direction_move;

    private const float SPEED_MOVE = 3f;

    private event System.Action onDeath;

    public void Initialize()
    {
        // Initialize enemy
        onDeath += () => EnemyController.Instance.OnEnemyKilled(this);
    }

    private void Update()
    {
        MoveTowardsPlayer();
        MoveUpdate();
    }

    private void MoveUpdate()
    {
        if (Moving)
        {
            Rigidbody.velocity = direction_move.normalized * SPEED_MOVE;
        }
        else
        {
            // Decelerate
            Rigidbody.velocity = Rigidbody.velocity * 0.7f;
        }
    }

    private void MoveTowardsPlayer()
    {
        if (!Player.Instance) return;

        Moving = true;
        direction_move = transform.DirectionTo(Player.Instance.transform);
        Character.SetLookDirection(direction_move);
    }

    public void Kill()
    {
        gameObject.SetActive(false);
        InstantiateParticle("Particles/ps_enemy_death")
            .Position(transform.position)
            .Destroy(1);

        // Event
        onDeath?.Invoke();
    }
}
