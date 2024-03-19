using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class AI_Jelly : EnemyAI
{
    public float max_distance_to_player;

    private bool moving;

    private bool IsTooClose => DistanceToPlayer() < max_distance_to_player;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        StartCoroutine(MoveCr());
    }

    private void FixedUpdate()
    {
        if (!moving)
        {
            Self.Rigidbody.velocity *= 0.98f;
        }
    }

    private IEnumerator MoveCr()
    {
        while (true)
        {
            Lerp.LocalScale(Self.Body.pivot_sprite, 0.5f, new Vector3(0.8f, 1.2f, 1f))
            .Curve(EasingCurves.EaseOutQuad);

            moving = true;
            var time_move = Time.time + 0.5f;
            while (Time.time < time_move && !IsTooClose)
            {
                Self.Move(Self.MoveDirection);
                yield return new WaitForFixedUpdate();
            }
            moving = false;

            Lerp.LocalScale(Self.Body.pivot_sprite, 1f, Vector3.one)
                .Curve(EasingCurves.EaseInOutQuad);

            while (IsTooClose)
            {
                MoveToStop();
                TurnTowards(PlayerPosition);
                yield return null;
            }

            var time_wait = Time.time + 1f;
            while (Time.time < time_wait)
            {
                TurnTowards(PlayerPosition);
                yield return new WaitForFixedUpdate();
            }
        }
    }
}