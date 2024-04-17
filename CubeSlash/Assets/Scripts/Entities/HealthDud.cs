using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class HealthDud : MonoBehaviour, IKillable
{
    [SerializeField] private GameObject pivot;
    [SerializeField] private GameObject idle_pivot;
    [SerializeField] private ParticleSystem ps_kill, ps_ooze, ps_glow;
    [SerializeField] private AnimationCurve ac_armor_active;
    [SerializeField] private AnimationCurve ac_armor_inactive;
    public bool Dead { get; private set; }
    public bool ArmorActive { get; private set; }
    public bool DudActive { get; private set; }

    public System.Action OnKilled;

    public void Initialize()
    {
        SetDudActive(true, false);
        Dead = false;
    }

    private void OnEnable()
    {
        AnimateIdle();
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

    public bool TryKill()
    {
        if (!CanKill()) return false;

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

        return true;
    }

    public bool CanHit() => IsAlive() && !ArmorActive && DudActive;
    public bool CanKill() => CanHit();
    public Vector3 GetPosition() => transform.position;

    public Coroutine AnimateIdle()
    {
        var start = Vector3.one * 0.9f;
        var end = Vector3.one * 1.1f;
        var curve = EasingCurves.EaseInOutQuad;
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            while (true)
            {
                yield return LerpEnumerator.LocalScale(idle_pivot.transform, Random.Range(0.2f, 0.3f), start).Curve(curve);
                yield return LerpEnumerator.LocalScale(idle_pivot.transform, Random.Range(0.2f, 0.3f), end).Curve(curve);
            }
        }
    }
}