using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_SearchHunt : EntityAI
{
    private enum State { SEARCH, HUNT }
    private State state;

    private Vector3 pos_target;

    private void Start()
    {
        UpdateState();
    }

    private void Update()
    {
        var dist_to_player = DistanceToPlayer();
        if (dist_to_player > CameraController.Instance.Width * 2f)
        {
            Self.Respawn();
            return;
        }

        switch (state)
        {
            case State.SEARCH:
                if (dist_to_player < CameraController.Instance.Width * 0.25f)
                {
                    SetState(State.HUNT);
                }
                else
                {
                    MoveNearPlayer();
                }
                break;
            case State.HUNT:
                MoveTowardsPlayer();
                break;
        }
    }

    private void SetState(State state)
    {
        this.state = state;
        if (state == State.SEARCH)
        {
            pos_target = GetPositionNearPlayer();
        }
    }

    public void UpdateState()
    {
        if (DistanceToPlayer() > CameraController.Instance.Width * 0.5f)
        {
            SetState(State.SEARCH);
        }
        else
        {
            SetState(State.HUNT);
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
            var direction = transform.DirectionTo(pos_target);
            Self.Move(direction);
        }
    }

    private void MoveTowardsPlayer()
    {
        if (!Player.Instance) return;

        var direction = transform.DirectionTo(Player.Instance.transform);
        Self.Move(direction);
    }
}
