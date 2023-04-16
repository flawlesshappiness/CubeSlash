using UnityEngine;
using Flawliz.Lerp;

public class HealthDud : MonoBehaviour, IKillable
{
    [SerializeField] private GameObject pivot;
    [SerializeField] private GameObject g_armor;
    [SerializeField] private ParticleSystem ps_kill, ps_ooze, ps_glow;
    [SerializeField] private AnimationCurve ac_armor_active;
    [SerializeField] private AnimationCurve ac_armor_inactive;
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

        SetGlowEnabled(active);

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

    public void SetGlowEnabled(bool enabled)
    {
        ps_glow.SetEmissionEnabled(enabled);
    }

    public bool IsAlive() => !Dead;

    public void Kill()
    {
        ps_kill.Duplicate()
            .Parent(GameController.Instance.world)
            .Scale(transform.localScale)
            .Play()
            .Destroy(5f);

        pivot.SetActive(false);
        ps_ooze.Play();
        SoundController.Instance.Play(SoundEffectType.sfx_dud_death);
        Dead = true;
        OnKilled?.Invoke();
    }

    public bool CanKill() => IsAlive() && !ArmorActive && DudActive;
    public Vector3 GetPosition() => transform.position;
}