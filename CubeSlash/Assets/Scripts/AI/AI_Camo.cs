using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class AI_Camo : EnemyAI
{
    public float min_distance;
    public float max_distance;

    public CamoBody CamoBody => Body as CamoBody;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);

        AreaController.Instance.onNextArea += OnNextArea;

        UpdateCurrentCamo();
        MoveSequence();
    }

    private Color GetCamoColor()
    {
        var area = AreaController.Instance.CurrentArea;
        var color = area.color_fog.SetA(1);
        return color;
    }

    private void OnNextArea(Area area)
    {
        CamoBody.AnimateCamoColor(GetCamoColor(), BackgroundController.OBJECT_FADE_TIME);
    }

    private void UpdateCurrentCamo()
    {
        CamoBody.SetCamoColor(GetCamoColor());
    }

    private Coroutine MoveSequence()
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            while (true)
            {
                if (DistanceToPlayer() > min_distance)
                {
                    CamoBody.AnimateCamoColor(GetCamoColor());
                }

                while (DistanceToPlayer() > min_distance)
                {
                    MoveTowards(PlayerPosition);
                    TurnTowards(PlayerPosition);
                    yield return new WaitForFixedUpdate();
                }

                // Telegraph
                var telegraph_duration = 0.5f;
                TelegraphSequence(telegraph_duration);
                CamoBody.AnimateRemoveCamoColor(telegraph_duration);

                SoundController.Instance.SetGroupVolumeByPosition(SoundEffectType.sfx_enemy_camo_appear, Position);
                SoundController.Instance.PlayGroup(SoundEffectType.sfx_enemy_camo_appear);

                // Stop
                var time_telegraph_end = Time.time + telegraph_duration;
                while (Time.time < time_telegraph_end)
                {
                    MoveToStop();
                    TurnTowards(PlayerPosition);
                    yield return new WaitForFixedUpdate();
                }

                // Charge
                var time_charge_end = Time.time + 2f;
                while (Time.time < time_charge_end)
                {
                    Self.AngularVelocity = 1;
                    Self.LinearAcceleration = 15;
                    Self.LinearVelocity = 50;
                    Self.Move(DirectionToPlayer());
                    TurnTowards(Position + Self.Rigidbody.velocity.normalized.ToVector3() * 1000);
                    yield return new WaitForFixedUpdate();
                }

                // Swim away a little
                while (DistanceToPlayer() < max_distance)
                {
                    var destination = Position - DirectionToPlayer().normalized * 1000;
                    MoveTowards(destination);
                    TurnTowards(destination);
                    yield return new WaitForFixedUpdate();
                }

                Self.AngularVelocity = Self.Settings.angular_velocity;
                Self.LinearAcceleration = Self.Settings.linear_acceleration;
                Self.LinearVelocity = Self.Settings.linear_velocity;
            }
        }
    }

    private Coroutine TelegraphSequence(float duration)
    {
        var duration_per = duration / 4;
        var q_left = Quaternion.AngleAxis(25f, Vector3.forward);
        var q_right = Quaternion.AngleAxis(-25f, Vector3.forward);
        var q_zero = Quaternion.AngleAxis(0, Vector3.forward);
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return LerpEnumerator.LocalRotation(Body.pivot_sprite, duration_per, q_left).Curve(EasingCurves.EaseInOutQuad);
            yield return LerpEnumerator.LocalRotation(Body.pivot_sprite, duration_per, q_right).Curve(EasingCurves.EaseInOutQuad);
            yield return LerpEnumerator.LocalRotation(Body.pivot_sprite, duration_per, q_left).Curve(EasingCurves.EaseInOutQuad);
            yield return LerpEnumerator.LocalRotation(Body.pivot_sprite, duration_per, q_zero).Curve(EasingCurves.EaseInOutQuad);
        }
    }
}