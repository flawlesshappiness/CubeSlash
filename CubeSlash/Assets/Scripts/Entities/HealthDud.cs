using UnityEngine;

public class HealthDud : MonoBehaviour, IKillable
{
    [SerializeField] private GameObject g_armor;
    [SerializeField] private AnimationCurve ac_armor_active;
    [SerializeField] private AnimationCurve ac_armor_inactive;
    public bool ArmorActive { get; private set; }

    public void Initialize()
    {
        g_armor.transform.localScale = Vector3.zero;
        ArmorActive = false;
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

    public bool IsActive() => gameObject.activeInHierarchy;

    public void Kill()
    {
        gameObject.SetActive(false);
    }

    public bool CanKill() => !ArmorActive;
}