using UnityEngine;

public class AI_SlowMove : EntityAI
{
    [SerializeField] private AnimationCurve ac_move_mul;
    [SerializeField] private AnimationCurve ac_turn_mul;


    private void Update()
    {
        if (!PlayerIsAlive()) return;

        var dist = DistanceToPlayer();
        var dist_max = CameraController.Instance.Width * 0.5f;
        var t_dist = dist / dist_max;
        var move = Self.Settings.speed_move * ac_move_mul.Evaluate(t_dist);
        var turn = Self.Settings.speed_turn * ac_turn_mul.Evaluate(t_dist);
        MoveTowards(Player.Instance.transform.position, move, turn);
    }
}