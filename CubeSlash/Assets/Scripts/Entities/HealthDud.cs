using UnityEngine;

public class HealthDud : MonoBehaviour, IKillable
{
    [SerializeField] private GameObject pivot;
    [SerializeField] private GameObject g_armor;
    [SerializeField] private ParticleSystem ps_kill;
    [SerializeField] private ParticleSystem ps_ooze;
    [SerializeField] private AnimationCurve ac_armor_active;
    [SerializeField] private AnimationCurve ac_armor_inactive;
    public bool Dead { get; private set; }
    public bool ArmorActive { get; private set; }
    public System.Action OnKilled;

    public void Initialize()
    {
        g_armor.transform.localScale = Vector3.zero;
        ArmorActive = false;
        Dead = false;
    }

    public void SetArmorActive(bool active, bool animate = true)
    {
        ArmorActive = active;

        var end = active ? Vector3.one : Vector3.zero;
        if (animate)
        {
            Lerp.Scale(g_armor.transform, 0.25f, end)
            .Curve(active ? ac_armor_active : ac_armor_inactive)
            .Unclamp();
        }
        else
        {
            g_armor.transform.localScale = end;
        }
    }

    public bool IsActive() => !Dead;

    public void Kill()
    {
        pivot.SetActive(false);
        ps_kill.Play();
        ps_ooze.Play();
        OnKilled?.Invoke();
        Dead = true;
    }

    public bool CanKill() => IsActive() && !ArmorActive;
}