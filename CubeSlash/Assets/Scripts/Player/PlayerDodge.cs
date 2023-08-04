using System.Collections;
using UnityEngine;

public class PlayerDodge : MonoBehaviour
{
    public bool Dashing { get; private set; }
    public float Distance { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.dodge_distance).ModifiedValue.float_value; } }
    private float Cooldown { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.dodge_cooldown).ModifiedValue.float_value; } }
    public Player Player { get { return Player.Instance; } }

    private float time_cooldown_start;
    private float time_cooldown_end;
    private float distance_dashed;
    private Vector3 dir_dash;
    private Coroutine cr_dash;

    private const float TIME = 0.2f;

    private void Start()
    {
        Player.onHurt += OnHurt;
    }

    private void OnHurt()
    {
        if (Dashing)
        {
            EndDash();
        }
    }

    public void Press()
    {
        StartDashing();
    }

    private void StartDashing()
    {
        if (IsOnCooldown()) return;
        if (Dashing) return;
        Dashing = true;

        Player.InputLock.AddLock(nameof(PlayerDodge));
        Player.DragLock.AddLock(nameof(PlayerDodge));
        Player.AbilityLock.AddLock(nameof(PlayerDodge));

        SoundController.Instance.Play(SoundEffectType.sfx_dash_start);

        cr_dash = StartCoroutine(DashCr(Player.MoveDirection));
    }

    private IEnumerator DashCr(Vector3 direction)
    {
        var speed = Calculator.DST_Speed(Distance, TIME);
        var velocity = direction * speed;
        var pos_prev = Player.transform.position;
        distance_dashed = 0f;
        dir_dash = direction;
        while (distance_dashed < Distance)
        {
            // Update distance
            var pos_cur = Player.transform.position;
            distance_dashed += Vector3.Distance(pos_prev, pos_cur);
            pos_prev = pos_cur;

            // Update direction
            var input = PlayerInput.MoveDirection;
            if (input.magnitude > 0.5f)
            {
                var right = Vector3.Cross(direction, Vector3.forward);
                var dot = Vector3.Dot(right, input);
                var sign = Mathf.Sign(dot);
                var angle = -2 * sign;
                direction = Quaternion.AngleAxis(angle, Vector3.forward) * direction;
                velocity = direction * speed;
            }

            Player.Rigidbody.velocity = velocity;
            Player.Body.SetLookDirection(direction);

            yield return new WaitForFixedUpdate();
        }

        EndDash();
    }

    private void EndDash()
    {
        StopCoroutine(cr_dash);
        cr_dash = null;

        Dashing = false;
        Player.InputLock.RemoveLock(nameof(PlayerDodge));
        Player.DragLock.RemoveLock(nameof(PlayerDodge));
        Player.AbilityLock.RemoveLock(nameof(PlayerDodge));

        StartCooldown();
    }

    private void StartCooldown()
    {
        time_cooldown_start = Time.time;
        time_cooldown_end = time_cooldown_start + Cooldown;
    }

    private bool IsOnCooldown() => Time.time < time_cooldown_end;

    public float GetCooldownPercentage() => time_cooldown_end == 0 ? 0 : 1f - Mathf.Clamp01((Time.time - time_cooldown_start) / (time_cooldown_end - time_cooldown_start));
}