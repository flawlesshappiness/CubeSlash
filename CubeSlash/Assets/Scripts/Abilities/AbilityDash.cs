using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDash : Ability
{
    private bool Dashing { get; set; }
    private float TimeStart { get; set; }

    [Header("DASH")]
    public AnimationCurve ac_path_normal;
    public AnimationCurve ac_path_split;

    public override void Pressed()
    {
        base.Pressed();
        if (Dashing) return;
        StartDashing(true, true, true);
    }

    public override void EnemyCollision(Enemy enemy)
    {
        base.EnemyCollision(enemy);

        if (!Dashing) return;
        DashHitEnemy(enemy);

        if (enemy.health > 0)
        {
            StopDashing();
            Player.MoveDirection = -Player.MoveDirection;
            StartDashing(true, false, true);
        }
        else
        {
            StartCoroutine(DashHitstopCr());
        }
    }

    private List<Enemy> _hits_dash = new List<Enemy>();
    private bool dash_hitstop = false;
    private void StartDashing(bool reset_hits, bool hit_start, bool hit_end)
    {
        if (reset_hits) _hits_dash.Clear();
        _cr_dash = StartCoroutine(DashPhaseCr(hit_start, hit_end));
    }

    private void StopDashing()
    {
        if (_cr_dash != null)
        {
            StopCoroutine(_cr_dash);
            _cr_dash = null;
        }

        Dashing = false;
    }

    private Coroutine _cr_dash;
    private IEnumerator DashPhaseCr(bool hit_start, bool hit_end)
    {
        Dashing = true;
        BlockingAbilities = true;
        BlockingMovement = true;

        // Add invincibility
        Player.InvincibilityCounter++;

        // Hit everyone around
        if (hit_start)
        {
            DashHitEnemiesArea(Player.transform.position, 1.5f);
        }

        // Dash
        var time = HasModifier(Type.SPLIT) ? 0.4f : 0.2f;
        var speed = HasModifier(Type.SPLIT) ? 30 : 40;
        Player.Character.SetLookDirection(Player.MoveDirection);
        TimeStart = Time.time;
        StartDashVisual(time);
        yield return DashCr(time, speed);

        // Hit everyone around
        if (hit_end)
        {
            DashHitEnemiesArea(Player.transform.position + Player.MoveDirection * 0.5f, 1.5f);
        }

        // Remove invincibility
        Player.InvincibilityCounter--;

        BlockingAbilities = false;
        BlockingMovement = false;
        Dashing = false;
    }

    private IEnumerator DashCr(float time, float speed)
    {
        while (Time.time - TimeStart < time)
        {
            if (dash_hitstop)
            {
                TimeStart += Time.deltaTime;
                Player.Rigidbody.velocity = Vector3.zero;
            }
            else
            {
                Player.Rigidbody.velocity = Player.MoveDirection * speed;
            }
            yield return new WaitForFixedUpdate();
        }
        Player.Rigidbody.velocity = Player.MoveDirection * Player.SPEED_MOVE;
    }

    private IEnumerator DashHitstopCr()
    {
        dash_hitstop = true;
        for (int i = 0; i < 10; i++)
        {
            yield return null;
        }
        dash_hitstop = false;
    }

    private Coroutine _cr_visual;
    private IEnumerator DashVisualCr(float time)
    {
        // Start
        Character clone = null;
        var curve = ac_path_normal;

        if (HasModifier(Type.SPLIT))
        {
            clone = GetClone();
            clone.gameObject.SetActive(true);
            clone.transform.localPosition = Player.Character.transform.localPosition;
            curve = ac_path_split;
        }

        // Dash
        while(Time.time - TimeStart < time)
        {
            var t = (Time.time - TimeStart) / time;
            var tval = curve.Evaluate(t);
            var dir = Player.MoveDirection;
            var right = Vector3.Cross(dir, Vector3.forward).normalized;
            Player.Character.transform.localPosition = right * tval;

            if (clone)
            {
                clone.transform.localPosition = right * (1 - tval);
            }

            yield return new WaitForFixedUpdate();
        }

        // End
        StopDashVisual();
    }

    private void StartDashVisual(float time)
    {
        StopDashVisual();
        _cr_visual = StartCoroutine(DashVisualCr(time));
    }

    private void StopDashVisual()
    {
        if(_cr_visual != null)
        {
            StopCoroutine(_cr_visual);
            _cr_visual = null;
        }

        // Player
        Player.Instance.Character.transform.localPosition = Vector3.zero;

        // Hide clone
        var clone = GetClone();
        if (clone)
        {
            clone.gameObject.SetActive(false);
        }
    }

    private void DashHitEnemy(Enemy enemy)
    {
        if (enemy && !_hits_dash.Contains(enemy))
        {
            _hits_dash.Add(enemy);
            Player.DamageEnemy(enemy, 1);
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

    #region CLONE
    private Character clone;
    private Character GetClone()
    {
        if(clone == null)
        {
            clone = Instantiate(Player.Character.gameObject, Player.Character.transform.parent).GetComponent<Character>();
        }
        return clone;
    }
    #endregion
}
