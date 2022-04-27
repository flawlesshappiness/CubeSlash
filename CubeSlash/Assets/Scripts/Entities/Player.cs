using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviourExtended
{
    public static Player Instance;
    private Character Character { get { return GetComponentOnce<Character>(ComponentSearchType.CHILDREN); } }
    private Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.CHILDREN); } }

    private const float SPEED_MOVE = 5;
    private const float SPEED_DASH = 40;
    private const float TIME_DASH = 0.2f;

    public System.Action<Enemy> onEnemyKilled;
    public System.Action<Enemy> onHurt;
    public bool Dashing { get; private set; }

    private Vector3 dir_move;

    private void Update()
    {
        InputUpdate();
        MoveUpdate();
    }

    private void InputUpdate()
    {
        if (Input.GetButtonDown("Fire3"))
        {
            StartDashing();
        }
    }

    private void StartDashing()
    {
        if (Dashing) return;
        _cr_dash = StartCoroutine(DashCr());
    }

    private void StopDashing()
    {
        if(_cr_dash != null)
        {
            StopCoroutine(_cr_dash);
            _cr_dash = null;
        }

        Dashing = false;
    }

    private Coroutine _cr_dash;
    private IEnumerator DashCr()
    {
        Dashing = true;

        var time_start = Time.time;
        while(Time.time - time_start < TIME_DASH)
        {
            Rigidbody.velocity = dir_move * SPEED_DASH;
            Character.SetLookDirection(dir_move);
            yield return null;
        }
        Rigidbody.velocity = dir_move * SPEED_MOVE;

        // Hit everyone around
        foreach(var hit in Physics2D.OverlapCircleAll(transform.position + dir_move * 0.5f, 1.5f))
        {
            var enemy = hit.GetComponentInParent<Enemy>();
            if (enemy)
            {
                enemy.Damage(1); // Could be a double hit
            }
        }

        Dashing = false;
    }

    #region MOVE
    private void MoveUpdate()
    {
        if (Dashing) return;

        var hor = Input.GetAxis("Horizontal");
        var ver = Input.GetAxis("Vertical");
        var dir = new Vector2(hor, ver);
        if(dir.magnitude > 0.5f)
        {
            dir_move = dir.normalized;
            Move(dir.normalized);
        }
        else
        {
            // Decelerate
            Rigidbody.velocity = Rigidbody.velocity * 0.7f;
        }
    }

    private void Move(Vector3 direction)
    {
        Rigidbody.velocity = direction * SPEED_MOVE;
        Character.SetLookDirection(direction);
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var enemy = collision.gameObject.GetComponentInParent<Enemy>();
        if (enemy)
        {
            if (Dashing)
            {
                enemy.Damage(1);

                if(enemy.health > 0)
                {
                    StopDashing();
                    dir_move = -dir_move;
                    StartDashing();

                    InstantiateParticle("Particles/ps_impact_dash")
                        .Position(collision.transform.position)
                        .Destroy(1)
                        .Play();
                }
                else
                {
                    onEnemyKilled?.Invoke(enemy);
                }
            }
            else
            {
                print("Player hit by enemy");
                onHurt?.Invoke(enemy);
            }
        }
    }
}
