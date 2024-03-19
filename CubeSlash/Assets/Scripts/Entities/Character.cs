using UnityEngine;

public abstract class Character : MonoBehaviourExtended
{
    public Body Body { get; private set; }
    public Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.THIS); } }
    public MultiLock MovementLock { get; set; } = new MultiLock();
    public MultiLock DragLock { get; set; } = new MultiLock();
    public MultiLock StunLock { get; set; } = new MultiLock();
    public float LinearAcceleration { get; set; }
    public float LinearVelocity { get; set; }
    public float LinearDrag { get; set; }
    public float AngularAcceleration { get; set; }
    public float AngularVelocity { get; set; }

    private float time_stun;

    protected virtual void OnUpdate() { }
    private void Update()
    {
        OnUpdate();
    }

    protected virtual void OnFixedUpdate() { }
    private void FixedUpdate()
    {
        DragUpdate();
        OnFixedUpdate();
    }

    protected void SetBody(Body prefab)
    {
        if (Body)
        {
            Destroy(Body.gameObject);
            Body = null;
        }

        Body = Instantiate(prefab.gameObject, transform).GetComponent<Body>();
        Body.Initialize();
    }

    public void Move(Vector3 direction)
    {
        if (MovementLock.IsFree && StunLock.IsFree)
        {
            Rigidbody.AddForce(direction.normalized * LinearAcceleration * Rigidbody.mass);
        }
    }

    public void MoveToStop(float mul = 1f)
    {
        if (StunLock.IsFree)
        {
            Rigidbody.AddForce(-Rigidbody.velocity * mul * Rigidbody.mass);
        }
    }

    private void DragUpdate()
    {
        if (IsStunned())
        {
            var max_velocity = Mathf.Max(0, LinearVelocity);
            var has_max_velocity = max_velocity > 0;
            if (Rigidbody.velocity.magnitude > max_velocity && has_max_velocity)
            {
                Rigidbody.AddForce(-Rigidbody.velocity.normalized * LinearDrag * Rigidbody.mass);
            }
            else if (Time.time > time_stun)
            {
                ResetStun();
            }
        }
        else if (DragLock.IsFree)
        {
            Rigidbody.velocity = Vector3.ClampMagnitude(Rigidbody.velocity, LinearVelocity);
            Rigidbody.angularVelocity = Mathf.Clamp(Rigidbody.angularVelocity, -AngularVelocity, AngularVelocity);
        }
    }

    public void Knockback(Vector3 velocity, bool use_mass = false, bool reset_velocity = false)
    {
        Stun();
        velocity *= use_mass ? Rigidbody.mass : 1;
        if (reset_velocity) Rigidbody.velocity = Vector3.zero;
        Rigidbody.AddForce(velocity);
    }

    public void Stun(float time = 0.2f)
    {
        StunLock.AddLock(nameof(Character));
        time_stun = Time.time + time;
    }

    public void ResetStun()
    {
        StunLock.RemoveLock(nameof(Character));
        time_stun = Time.time;
    }

    public bool IsStunned()
    {
        return StunLock.IsLocked || Time.time < time_stun;
    }
}