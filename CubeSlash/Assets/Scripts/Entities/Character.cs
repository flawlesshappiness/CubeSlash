using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Character : MonoBehaviourExtended
{
    public Body Body { get; private set; }
    public Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.THIS); } }
    public MultiLock MovementLock { get; set; } = new MultiLock();
    public MultiLock DragLock { get; set; } = new MultiLock();
    public MultiLock StunLock { get; set; } = new MultiLock();
    public float LinearAcceleration { get; set; }
    public float LinearVelocity { get; set; }
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
        if (StunLock.IsLocked || Time.time < time_stun)
        {
            if (Rigidbody.velocity.magnitude > LinearVelocity)
            {
                Rigidbody.AddForce(-Rigidbody.velocity.normalized * LinearAcceleration * Rigidbody.mass);
            }
            else if(Time.time > time_stun)
            {
                StunLock.RemoveLock(nameof(Character));
            }
        }
        else if (DragLock.IsFree)
        {
            Rigidbody.velocity = Vector3.ClampMagnitude(Rigidbody.velocity, LinearVelocity);
            Rigidbody.angularVelocity = Mathf.Clamp(Rigidbody.angularVelocity, -AngularVelocity, AngularVelocity);
        }
    }

    public void Knockback(Vector3 direction, bool use_mass = false, bool reset_velocity = false)
    {
        Stun();
        direction *= use_mass ? Rigidbody.mass : 1;
        if (reset_velocity) Rigidbody.velocity = Vector3.zero;
        Rigidbody.AddForce(direction);
    }

    public void Stun(float time = 0.2f)
    {
        StunLock.AddLock(nameof(Character));
        time_stun = Time.time + time;
    }
}