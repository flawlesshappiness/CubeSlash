using Flawliz.Lerp;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RadialMenuElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image image_icon;
    [SerializeField] private RectTransform pivot_idle, pivot_select;
    public enum AnimationType { ScaleBounce }
    public AnimationType animation_type;

    [SerializeField] private IdleAnimationInfo idle_animation_info;

    public RadialMenuOption Option { get; private set; }

    private RadialMenu menu;

    [System.Serializable]
    public class IdleAnimationInfo
    {
        public float position_frequency = 0.4f;
        public float position_scale = 10f;
        public float scale_min = 0.95f;
        public float scale_max = 1.0f;
        public float scale_duration_min = 0.2f;
        public float scale_duration_max = 0.3f;
    }

    private void Awake()
    {
        menu = GetComponentInParent<RadialMenu>();
        SetHidden();
    }

    private void Start()
    {
        StartCoroutine(AnimateIdleScaleCr());
        StartCoroutine(AnimateIdlePositionCr());
    }

    public void Initialize(RadialMenuOption option)
    {
        Option = option;
        image_icon.sprite = option.Sprite;
    }

    public void SetHidden() => transform.localScale = Vector3.zero;
    public void SetVisible() => transform.localScale = Vector3.one;

    public void OnPointerClick(PointerEventData eventData)
    {
        menu.BeginSubmit();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(menu.CurrentElement == this)
        {
            menu.SetCurrentElement(null);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        menu.SetCurrentElement(this);
    }

    private IEnumerator AnimateIdleScaleCr()
    {
        var start = Vector3.one * idle_animation_info.scale_min;
        var end = Vector3.one * idle_animation_info.scale_max;
        while (true)
        {
            var duration = Random.Range(idle_animation_info.scale_duration_min, idle_animation_info.scale_duration_max);
            yield return LerpEnumerator.LocalScale(pivot_idle, duration, start, end).Curve(EasingCurves.EaseInOutSine).UnscaledTime();
            yield return LerpEnumerator.LocalScale(pivot_idle, duration, end, start).Curve(EasingCurves.EaseInOutSine).UnscaledTime();
        }
    }

    private IEnumerator AnimateIdlePositionCr()
    {
        var offset = new Vector2(Random.Range(0f, 100f), Random.Range(0f, 100f));
        var scale = idle_animation_info.position_scale;
        var freq = idle_animation_info.position_frequency;
        while (true)
        {
            var time = Time.time * freq;
            var x = Mathf.PerlinNoise(time + offset.x, 0) * 2f - 1f;
            var y = Mathf.PerlinNoise(0f, time + offset.y) * 2f - 1f;
            pivot_idle.localPosition = new Vector2(x, y) * scale;
            yield return null;
        }
    }

    public CustomCoroutine AnimateSelect(bool selected)
    {
        switch (animation_type)
        {
            case AnimationType.ScaleBounce: return AnimateSelectBounce(selected);
            default: return null;
        }
    }

    public CustomCoroutine AnimateSelectBounce(bool selected)
    {
        return this.StartCoroutineWithID(Cr(), "animate_select_" + GetInstanceID());
        IEnumerator Cr()
        {
            var end = selected ? 1.25f : 1f;
            var curve = selected ? EasingCurves.EaseOutBack : EasingCurves.EaseOutBack;
            yield return LerpEnumerator.LocalScale(pivot_select, 0.25f, Vector3.one * end)
                .Curve(curve)
                .UnscaledTime();
        }
    }

    public CustomCoroutine AnimateSubmit()
    {
        return this.StartCoroutineWithID(Cr(), "animate_" + GetInstanceID());
        IEnumerator Cr()
        {
            yield return LerpEnumerator.LocalScale(transform, 0.1f, Vector3.one * 1.4f).Curve(EasingCurves.EaseOutQuad).UnscaledTime();
            yield return LerpEnumerator.LocalScale(transform, 0.25f, Vector3.zero).Curve(EasingCurves.EaseInQuad).UnscaledTime();
        }
    }

    public CustomCoroutine AnimateShow(bool show)
    {
        return this.StartCoroutineWithID(Cr(), "animate_" + GetInstanceID());
        IEnumerator Cr()
        {
            var end = Vector3.one * (show ? 1f : 0f);
            var curve = show ? EasingCurves.EaseOutBack : EasingCurves.EaseInQuad;
            yield return LerpEnumerator.LocalScale(transform, 0.25f, end)
                .Curve(curve)
                .UnscaledTime();
        }
    }
}