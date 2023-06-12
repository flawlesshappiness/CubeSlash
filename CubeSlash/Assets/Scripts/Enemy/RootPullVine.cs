using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class RootPullVine : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spr_main;
    [SerializeField] private Transform pivot_main;
    [SerializeField] private ParticleSystem ps_pull, ps_dissolve;

    public float pull_force;
    public float drawmode_size = 0.32f;
    public float drawmode_position = 0.16f;
    [Min(0)] public float scale;


    public Rigidbody2D target;

    private float SpriteScale { get { return spr_main.transform.lossyScale.y; } }
    private float UnitToScale { get { return 1f / (drawmode_position * SpriteScale * 2); } }
    private float DistanceToTarget { get { return Vector3.Distance(transform.position, target.transform.position); } }

    private bool animating;

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        if (target != null) UpdateScale();
        UpdateVisual();
    }

    private void Update()
    {
        if (animating) return;
        UpdateScale();
        UpdateVisual();
        UpdateForce();
    }

    private void UpdateVisual()
    {
        UpdateSize();
        UpdateRotation();
        UpdateParticles();
    }

    private void UpdateScale()
    {
        scale = GetScaleToTarget();
    }

    private float GetScaleToTarget() => target == null ? 0 : DistanceToTarget * UnitToScale;

    private void UpdateSize()
    {
        if (spr_main == null) return;
        spr_main.size = new Vector2(drawmode_size, drawmode_size * scale);
        spr_main.transform.localPosition = new Vector3(0, drawmode_position * spr_main.transform.localScale.y * scale);
    }

    private void UpdateRotation()
    {
        if (pivot_main == null) return;
        if (target == null) return;
        pivot_main.rotation = transform.RotationTo(target.transform);
    }

    private void UpdateForce()
    {
        if(target == null) return;
        var dir = transform.DirectionTo(target.transform);
        target.AddForce(-dir * pull_force);
    }

    private void UpdateParticles()
    {
        if(target == null)
        {
            ps_pull.SetEmissionEnabled(false);
            return;
        }

        if (ps_pull == null) return;

        ps_pull.SetEmissionEnabled(true);

        var dist = DistanceToTarget;
        ps_pull.ModifyShape(m =>
        {
            m.scale = m.scale.SetZ(dist);
            m.position = m.position.SetY(dist * 0.5f);
        });
        ps_pull.ModifyEmission(m =>
        {
            m.rateOverTime = dist * 2;
        });
    }

    public void PlayDissolveFX()
    {
        if (ps_dissolve == null) return;
        if (target == null) return;

        var dist = DistanceToTarget;
        ps_dissolve.ModifyShape(m =>
        {
            m.scale = m.scale.SetZ(dist);
            m.position = m.position.SetY(dist * 0.5f);
        });
        ps_dissolve.ModifyEmission(m =>
        {
            m.SetBurst(0, new ParticleSystem.Burst(0, 5 * dist));
        });

        ps_dissolve.Duplicate()
            .Parent(GameController.Instance.world)
            .Destroy(5f)
            .Play();
    }

    public Coroutine AnimateToTarget()
    {
        if (target == null) return null;

        return StartCoroutine(Cr());

        IEnumerator Cr()
        {
            animating = true;
            var curve = EasingCurves.EaseOutQuad;
            var start_scale = scale;
            yield return LerpEnumerator.Value(0.5f, f =>
            {
                var t = curve.Evaluate(f);
                scale = Mathf.Lerp(start_scale, GetScaleToTarget(), t);
                UpdateVisual();
            });
            animating = false;
        }
    }

    public Coroutine AnimateFromTarget()
    {
        if (target == null) return null;

        return StartCoroutine(Cr());

        IEnumerator Cr()
        {
            animating = true;
            var curve = EasingCurves.EaseInQuad;
            var scale_start = GetScaleToTarget();
            yield return LerpEnumerator.Value(0.5f, f =>
            {
                var t = curve.Evaluate(f);
                scale = Mathf.Lerp(scale_start, 0f, t);
                UpdateVisual();
            });
            animating = false;
        }
    }
}