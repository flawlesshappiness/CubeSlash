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

    public bool Dashing { get; private set; }

    private Vector3 dir_move;

    private void Update()
    {
        InputUpdate();
        MoveUpdate();
    }

    #region INPUT
    private void InputUpdate()
    {
        if (Input.GetButtonDown("Fire3"))
        {
            Dash();
        }
    }

    private void Dash()
    {
        if (Dashing) return;
        StartCoroutine(DashCr());
    }

    private IEnumerator DashCr()
    {
        Dashing = true;

        var time_start = Time.time;
        while(Time.time - time_start < TIME_DASH)
        {
            Rigidbody.velocity = dir_move * SPEED_DASH;
            yield return null;
        }
        Rigidbody.velocity = dir_move * SPEED_MOVE;

        Dashing = false;
    }
    #endregion
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
                enemy.Kill();
            }
            else
            {
                print("Player hit by enemy");
            }
        }
    }
}
