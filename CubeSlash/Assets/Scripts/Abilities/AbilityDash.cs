using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityDash : Ability
{
    private bool Dashing { get; set; }
    private bool Reflects { get; set; }
    private bool Hitstop { get; set; }
    private float TimeStart { get; set; }
    private float TimeDash { get; set; }
    private float SpeedDash { get; set; }
    private float RadiusDash { get; set; }
    private int DamageDash { get; set; }

    [Header("DASH")]
    public AnimationCurve ac_path_normal;
    public AnimationCurve ac_path_split;

    public override void Pressed()
    {
        base.Pressed();

        var charge = GetModifier<AbilityCharge>(Type.CHARGE);
        if (charge)
        {
            charge.Pressed();
        }
        else
        {
            if (Dashing) return;
            StartDashing(true, true, true);
        }
    }

    
    public override void Released()
    {
        base.Released();

        var charge = GetModifier<AbilityCharge>(Type.CHARGE);
        if (charge)
        {
            var t = charge.GetCharge();
            charge.Released();

            if (Dashing) return;
            StartDashing(true, true, true);
        }
    }

    public override void EnemyCollision(Enemy enemy)
    {
        base.EnemyCollision(enemy);

        if (!Dashing) return;
        var already_hit = _hits_dash.Contains(enemy);
        DashHitEnemy(enemy);

        if (!already_hit && enemy.health > 0 && Reflects)
        {
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

        var charge = GetModifier<AbilityCharge>(Type.CHARGE);
        var t_charge = charge ? charge.GetCharge() : 0;
        TimeDash =
            HasModifier(Type.CHARGE) ? (t_charge == 1 ? 0.03f : Mathf.Lerp(0.05f, 0.2f, t_charge)) :
            HasModifier(Type.SPLIT) ? 0.4f :
            0.2f;
        SpeedDash =
            HasModifier(Type.CHARGE) ? (t_charge == 1 ? 240 : Mathf.Lerp(40, 50, t_charge)) :
            HasModifier(Type.SPLIT) ? 30 :
            40;
        DamageDash =
            charge && t_charge == 1 ? 3 :
            1;
        RadiusDash =
            charge && t_charge == 1 ? 2 :
            1;
        Reflects = !charge || t_charge < 1;
        Hitstop = !charge || t_charge < 1;
        CoroutineController.Instance.Run(DashPhaseCr(hit_start, hit_end), "dash_"+GetInstanceID());
    }

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
        Player.Character.SetLookDirection(Player.MoveDirection);
        TimeStart = Time.time;
        StartDashVisual(TimeDash);

        var charge = GetModifier<AbilityCharge>(Type.CHARGE);
        if (charge && charge.GetCharge() == 1)
        {
            yield return DashChargedCr();
        }
        else
        {
            yield return DashCr(TimeDash, SpeedDash);
        }

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

    private IEnumerator DashChargedCr()
    {
        var dir = Player.MoveDirection;
        var dist = 6f;
        var speed = 100f;
        var start = Player.transform.position;
        Player.transform.position = Player.transform.position + dir.normalized * dist;
        Player.Rigidbody.velocity = dir * speed;

        var hits = Physics2D.CircleCastAll(start, 2f, dir);
        foreach(var hit in hits)
        {
            var e = hit.collider.GetComponentInParent<Enemy>();
            DashHitEnemy(e);
        }

        while(Player.Rigidbody.velocity.magnitude > Player.SPEED_MOVE)
        {
            Player.Rigidbody.velocity *= 0.8f;
            yield return new WaitForFixedUpdate();
        }
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

    #region VISUAL
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
    }

    private void StartDashVisual(float time)
    {
        CoroutineController.Instance.Run(DashVisualCr(time), "dash_visual_" + GetInstanceID())
            .OnEnd(() =>
            {
                // Player
                Player.Instance.Character.transform.localPosition = Vector3.zero;

                // Hide clone
                var clone = GetClone();
                if (clone)
                {
                    clone.gameObject.SetActive(false);
                }
            });
    }
    #endregion
    #region ENEMY
    private void DashHitEnemy(Enemy enemy)
    {
        if (enemy && !_hits_dash.Contains(enemy))
        {
            _hits_dash.Add(enemy);
            if(DamageDash > 0)
            {
                Player.DamageEnemy(enemy, DamageDash);
            }
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
