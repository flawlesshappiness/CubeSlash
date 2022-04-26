using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviourExtended
{
    public enum Type { WEAK, CHONK }

    [Min(1)] public int health;

    private Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.CHILDREN); } }
    private Character Character { get; set; }
    private EntityAI AI { get; set; }

    public bool Moving { get; private set; }
    private Vector3 direction_move;
    private Vector3 pos_target;

    private const float SPEED_MOVE = 3f;

    private event System.Action onDeath;

    private enum StateAI { SEARCH, HUNT }
    private StateAI state;

    public void Initialize(EnemySettings settings)
    {
        health = settings.health;
        SetCharacter(settings.character);
        SetAI(settings.ai);
        transform.localScale = settings.size;
    }

    private void Update()
    {
        if (!Character) return;

        AIUpdate();
        MoveUpdate();
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
    }

    private void MoveUpdate()
    {
        if (Moving)
        {
            Rigidbody.velocity = direction_move.normalized * SPEED_MOVE;
            Character.SetLookDirection(direction_move);
        }
        else
        {
            // Decelerate
            Rigidbody.velocity = Rigidbody.velocity * 0.7f;
        }
    }

    #region AI
    private void AIUpdate()
    {
        var dist_to_player = DistanceToPlayer();
        if(dist_to_player > CameraController.Instance.Width * 2f)
        {
            Respawn();
            return;
        }

        switch (state)
        {
            case StateAI.SEARCH:
                if(dist_to_player < CameraController.Instance.Width * 0.25f)
                {
                    SetState(StateAI.HUNT);
                }
                else
                {
                    MoveNearPlayer();
                }
                break;
            case StateAI.HUNT:
                MoveTowardsPlayer();
                break;
        }
    }

    private void SetState(StateAI state)
    {
        this.state = state;
        if(state == StateAI.SEARCH)
        {
            pos_target = GetPositionNearPlayer();
        }
    }

    public void UpdateState()
    {
        if (DistanceToPlayer() > CameraController.Instance.Width * 0.5f)
        {
            SetState(StateAI.SEARCH);
        }
        else
        {
            SetState(StateAI.HUNT);
        }
    }

    private void MoveNearPlayer()
    {
        if (!Player.Instance) return;

        if (Vector3.Distance(transform.position, pos_target) < 1f)
        {
            UpdateState();
        }
        else
        {
            Moving = true;
            direction_move = transform.DirectionTo(pos_target);
        }
    }

    private void MoveTowardsPlayer()
    {
        if (!Player.Instance) return;

        Moving = true;
        direction_move = transform.DirectionTo(Player.Instance.transform);
    }

    private Vector3 GetPositionNearPlayer()
    {
        return Player.Instance.transform.position + Random.insideUnitCircle.ToVector3() * Random.Range(2f, 5f);
    }

    private float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, Player.Instance.transform.position);
    }
    #endregion

    public void Kill()
    {
        InstantiateParticle("Particles/ps_enemy_death")
            .Position(transform.position)
            .Destroy(1);

        // Event
        onDeath?.Invoke();

        // Respawn
        Respawn();
    }

    private void Respawn()
    {
        gameObject.SetActive(false);
        EnemyController.Instance.OnEnemyKilled(this);
    }
}
