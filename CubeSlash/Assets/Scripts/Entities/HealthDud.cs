using UnityEngine;
using Flawliz.Lerp;

public class HealthDud : MonoBehaviour, IKillable
{
    [SerializeField] private GameObject pivot;
    [SerializeField] private GameObject g_armor;
    [SerializeField] private ParticleSystem ps_kill;
    [SerializeField] private ParticleSystem ps_ooze;
    [SerializeField] private AnimationCurve ac_armor_active;
    [SerializeField] private AnimationCurve ac_armor_inactive;
    [SerializeField] private FMODEventReference sfx_death;
    public bool Dead { get; private set; }
    public bool ArmorActive { get; private set; }
    public bool DudActive { get; private set; }
    public System.Action OnKilled;

    public void Initialize()
    {
        SetArmorActive(false, false);
        SetDudActive(true, false);
        Dead = false;
    }

    public void SetArmorActive(bool active, bool animate = true)
    {
        ArmorActive = active;

        var end = active ? Vector3.one : Vector3.zero;
        if (animate)
        {
            var curve = active ? ac_armor_active : ac_armor_inactive;
            Lerp.LocalScale(g_armor.transform, 0.25f, end)
                .Curve(curve);
        }
        else
        {
            g_armor.transform.localScale = end;
        }
    }

    public void SetDudActive(bool active, bool animate = true)
    {
        DudActive = active;

        var end = active ? Vector3.one : Vector3.zero;
        if (animate)
        {
            var curve = active ? ac_armor_active : ac_armor_inactive;
            Lerp.LocalScale(pivot.transform, 0.25f, end)
                .Curve(curve);
        }
        else
        {
            pivot.transform.localScale = end;
        }
    }

    public bool IsActive() => !Dead;

    public void Kill()
    {
        pivot.SetActive(false);
        ps_kill.Play();
        ps_ooze.Play();
        sfx_death.Play();
        OnKilled?.Invoke();
        Dead = true;
    }

    public bool CanKill() => IsActive() && !ArmorActive && DudActive;
    public Vector3 GetPosition() => transform.position;
}