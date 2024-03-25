using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class PlantPillar : Obstacle
{
    [SerializeField] private Transform pivot_animation;
    [SerializeField] private Collider2D collider, trigger;
    [SerializeField] private ParticleSystem ps_warn, ps_appear, ps_death;

    public SoundEffectType sfx_telegraph;
    public SoundEffectType sfx_spawn;

    private bool is_hidden;

    public void SetHidden()
    {
        is_hidden = true;
        collider.enabled = false;
        trigger.enabled = false;
        pivot_animation.localScale = Vector3.zero;
    }

    public CustomCoroutine AnimateAppear(float delay_appear, float delay_disappear)
    {
        return this.StartCoroutineWithID(Cr(), "appear_" + GetInstanceID());

        IEnumerator Cr()
        {
            SoundController.Instance.SetGroupVolumeByPosition(sfx_telegraph, transform.position);
            SoundController.Instance.PlayGroup(sfx_telegraph);
            ps_warn.SetEmissionEnabled(true);
            yield return new WaitForSeconds(delay_appear);
            ps_warn.SetEmissionEnabled(false);
            ps_appear.Play();
            SoundController.Instance.SetGroupVolumeByPosition(sfx_spawn, transform.position);
            SoundController.Instance.PlayGroup(sfx_spawn);

            is_hidden = false;

            var angle_start = Random.Range(0f, 360f);
            var angle_end = angle_start + 90f;
            var scale_start = Vector3.zero;
            var scale_end = Vector3.one;
            var curve_appear = EasingCurves.EaseOutBack;

            Lerp.Value(0.25f, f =>
            {
                var t = curve_appear.Evaluate(f);
                var angle = Mathf.LerpUnclamped(angle_start, angle_end, t);
                pivot_animation.eulerAngles = new Vector3(0f, 0f, angle);
                pivot_animation.localScale = Vector3.LerpUnclamped(scale_start, scale_end, t);
            }).Connect(gameObject);

            yield return new WaitForSeconds(0.15f);

            collider.enabled = true;
            trigger.enabled = true;

            yield return new WaitForSeconds(0.1f);

            yield return new WaitForSeconds(delay_disappear);

            collider.enabled = false;
            trigger.enabled = false;

            Lerp.Value(0.25f, f =>
            {
                var angle = Mathf.LerpUnclamped(angle_end, angle_start, f);
                pivot_animation.eulerAngles = new Vector3(0f, 0f, angle);
                pivot_animation.localScale = Vector3.LerpUnclamped(scale_end, scale_start, f);
            }).Connect(gameObject);

            yield return new WaitForSeconds(0.25f);

            Destroy(gameObject);
        }
    }

    public void Kill()
    {
        if (is_hidden) return;

        ps_death.Duplicate()
            .Parent(GameController.Instance.world)
            .Play()
            .Destroy(10);

        Destroy(gameObject);
    }
}