using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviourExtended
{
    public static Player Instance;
    private Character Character { get { return GetComponentOnce<Character>(ComponentSearchType.CHILDREN); } }
    private Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.CHILDREN); } }
    public MinMaxInt Experience { get; private set; } = new MinMaxInt();
    public int Level { get; private set; }

    private const float SPEED_MOVE = 5;
    private const float SPEED_DASH = 40;
    private const float TIME_DASH = 0.2f;

    public System.Action<Enemy> onEnemyKilled;
    public System.Action<Enemy> onHurt;
    public bool Dashing { get; private set; }

    private Vector3 dir_move;

    public void Initialize()
    {
        Experience.Min = 0;
        Experience.Max = 25;
        Experience.Value = 0;
    }

    private void Update()
    {
        InputUpdate();
        MoveUpdate();
    }

    private void InputUpdate()
    {
        if (Input.GetButtonDown("Fire3"))
        {
            StartDashing(true, true, true);
        }
    }
    #region DASH
    private List<Enemy> _hits_dash = new List<Enemy>();
    private bool dash_hitstop = false;
    private void StartDashing(bool reset_hits, bool hit_start, bool hit_end)
    {
        if (Dashing) return;
        if(reset_hits) _hits_dash.Clear();
        _cr_dash = StartCoroutine(DashCr(hit_start, hit_end));
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
    private IEnumerator DashCr(bool hit_start, bool hit_end)
    {
        Dashing = true;

        // Hit everyone around
        if (hit_start)
        {
            DashHitEnemiesArea(transform.position, 1.5f);
        }

        // Dash
        var time_start = Time.time;
        while(Time.time - time_start < TIME_DASH)
        {
            if (dash_hitstop)
            {
                time_start += Time.deltaTime;
                Rigidbody.velocity = Vector3.zero;
            }
            else
            {
                Rigidbody.velocity = dir_move * SPEED_DASH;
                Character.SetLookDirection(dir_move);
            }
            yield return null;
        }
        Rigidbody.velocity = dir_move * SPEED_MOVE;

        // Hit everyone around
        if (hit_end)
        {
            DashHitEnemiesArea(transform.position + dir_move * 0.5f, 1.5f);
        }

        Dashing = false;
    }

    private IEnumerator DashHitstopCr()
    {
        dash_hitstop = true;
        for (int i = 0; i < 20; i++)
        {
            yield return null;
        }
        dash_hitstop = false;
    }

    private void DashHitEnemy(Enemy enemy)
    {
        if (enemy && !_hits_dash.Contains(enemy))
        {
            _hits_dash.Add(enemy);
            DamageEnemy(enemy, 1);
        }
    }

    private void DashHitEnemiesArea(Vector3 position, float radius)
    {
        foreach (var hit in Physics2D.OverlapCircleAll(position, radius))
        {
            var enemy = hit.GetComponentInParent<Enemy>();
            DashHitEnemy(enemy);
        }
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
    #region ENEMY
    private void DamageEnemy(Enemy enemy, int damage)
    {
        enemy.Damage(damage);

        if(enemy.health > 0)
        {
            InstantiateParticle("Particles/ps_impact_dash")
                        .Position(enemy.transform.position)
                        .Destroy(1)
                        .Play();
        }
        else
        {
            Experience.Value += 1;
            onEnemyKilled?.Invoke(enemy);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var enemy = collision.gameObject.GetComponentInParent<Enemy>();
        if (enemy)
        {
            if (Dashing)
            {
                DashHitEnemy(enemy);

                if (enemy.health > 0)
                {
                    StopDashing();
                    dir_move = -dir_move;
                    StartDashing(true, false, true);
                }
                else
                {
                    StartCoroutine(DashHitstopCr());
                }
            }
            else
            {
                print("Player hit by enemy");
                onHurt?.Invoke(enemy);
            }
        }
    }
    #endregion
}
