using System.Collections;
using UnityEngine;

public class Enemy : Character, IKillable, IHurt
{
    private EnemyAI AI { get; set; }
    public EnemySettings Settings { get; private set; }
    public Vector3 MoveDirection { get { return Body.transform.up; } }

    public bool IsBoss { get; set; }
    public bool IsDead { get; private set; }
    public bool CanReposition { get; set; }
    public MultiLock InvincibleLock { get; private set; } = new MultiLock();
    public MultiLock HitLock { get; private set; } = new MultiLock();

    public float SpeedMultiplier => RunInfo.Current.Endless ? Mathf.Lerp(1f, 2f, GamemodeController.Instance.SelectedGameMode.T_GameDuration) : 1f;

    public event System.Action OnHit;
    public event System.Action OnDeath;

    private float time_spawn;

    public void Initialize(EnemySettings settings)
    {
        IsDead = false;
        IsBoss = false;
        OnDeath = null;
        OnHit = null;
        CanReposition = true;

        InvincibleLock.ClearLock();
        HitLock.ClearLock();

        this.Settings = settings;
        LinearAcceleration = settings.linear_acceleration * SpeedMultiplier;
        LinearVelocity = settings.linear_velocity * SpeedMultiplier;
        LinearDrag = settings.linear_drag;
        AngularAcceleration = settings.angular_acceleration;
        AngularVelocity = settings.angular_velocity;

        transform.localScale = Vector3.one * settings.size;
        Rigidbody.mass = settings.mass;
        Rigidbody.isKinematic = false;

        SetBody(settings.body);
        SetAI(settings.ai);

        time_spawn = Time.time + 0.1f;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        RepositionUpdate();
    }

    public Vector3 GetPosition() => transform.position;

    public void RepositionUpdate()
    {
        if (Player.Instance == null) return;
        if (!CanReposition) return;

        var size = GamemodeController.Instance.SelectedGameMode.area_size;
        var sh = size * 0.5f;
        var center = Player.Instance.transform.position;
        var pos = transform.position;

        if (pos.x < center.x - sh) transform.position += new Vector3(size, 0);
        if (pos.x > center.x + sh) transform.position -= new Vector3(size, 0);
        if (pos.y < center.y - sh) transform.position += new Vector3(0, size);
        if (pos.y > center.y + sh) transform.position -= new Vector3(0, size);
    }

    private void SetAI(EnemyAI prefab)
    {
        if (AI)
        {
            AI.Kill();
            AI = null;
        }

        if (prefab)
        {
            AI = Instantiate(prefab.gameObject, transform).GetComponent<EnemyAI>();
            AI.Initialize(this);
        }
    }

    public void Turn(bool right)
    {
        var angle = right ? -1 : 1;
        Rigidbody.AddTorque(angle * AngularAcceleration * Rigidbody.mass);
    }
    #region HEALTH
    public virtual bool CanHit() => !HasSpawnProtection() && HitLock.IsFree && !IsBoss && !IsDead;
    public virtual bool CanKill() => CanHit() && InvincibleLock.IsFree;
    public bool HasSpawnProtection() => Time.time < time_spawn;

    public bool TryKill()
    {
        if (IsDead) return false;

        OnHit?.Invoke();

        if (!CanKill()) return false;

        Kill();

        return true;
    }

    public void Kill(bool callOnDeathEvents = true)
    {
        IsDead = true;

        // PS
        if (Body.ps_death != null)
        {
            var psd = Body.ps_death.Duplicate()
                .Position(transform.position)
                .Scale(Vector3.one * Settings.size)
                .Destroy(15)
                .Play();

            ObjectController.Instance.Add(psd.ps.gameObject);
        }

        var sfx = SoundEffectType.sfx_enemy_death;
        SoundController.Instance.SetGroupVolumeByPosition(sfx, transform.position);
        SoundController.Instance.PlayGroup(sfx);

        // Event
        if (callOnDeathEvents)
        {
            OnDeath?.Invoke();
        }

        // AI
        SetAI(null);

        // Respawn
        Respawn();
    }

    public void Respawn()
    {
        gameObject.SetActive(false);
        EnemyController.Instance.EnemyKilled(this);
    }

    public void SetInvincible(string id, float duration)
    {
        this.StartCoroutineWithID(Cr(), "invincible_" + GetInstanceID());
        IEnumerator Cr()
        {
            InvincibleLock.AddLock(id);
            yield return new WaitForSeconds(duration);
            InvincibleLock.RemoveLock(id);
        }
    }
    #endregion
    #region IHURT
    public bool CanHurt() => true;
    #endregion
}
