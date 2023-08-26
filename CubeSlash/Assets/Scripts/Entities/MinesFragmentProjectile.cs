using UnityEngine;

public class MinesFragmentProjectile : Projectile
{
    public float CurveAngle { get; set; }

    public override void Update()
    {
        base.Update();
        UpdateRotation();
    }

    private void UpdateRotation()
    {
        var angle = CurveAngle * Time.deltaTime;
        var q = Quaternion.AngleAxis(angle, Vector3.forward);
        pivot_animation.rotation *= q;
        Rigidbody.velocity = q * Rigidbody.velocity;
    }
}