using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UnlockItemView : View
{
    [SerializeField] private UIInputLayout input;
    [SerializeField] private Image img_item;
    [SerializeField] private CanvasGroup cvg_fx, cvg_title;
    [SerializeField] private RectTransform rt_rays, rt_glow, rt_item;
    [SerializeField] private TMP_Text tmp_title;

    public System.Action OnSubmit;

    private bool animating = true;

    private void Awake()
    {
        input.Clear();
        input.AddInput(PlayerInput.UIButtonType.SOUTH, "Claim");

        input.CanvasGroup.alpha = 0;
        cvg_title.alpha = 0;
        cvg_fx.alpha = 0;

        rt_rays.localScale = Vector3.zero;
        rt_glow.localScale = Vector3.zero;
        rt_item.localScale = Vector3.zero;

        AnimateRaysRotation();
    }

    private void OnEnable()
    {
        PlayerInput.Controls.UI.Submit.started += ClickClaim;
    }

    private void OnDisable()
    {
        PlayerInput.Controls.UI.Submit.started -= ClickClaim;
    }

    private void ClickClaim(InputAction.CallbackContext ctx)
    {
        if (animating) return;
        animating = true;

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            SoundController.Instance.Play(SoundEffectType.sfx_ui_marima_001);
            Lerp.LocalScale(rt_rays, 0.25f, Vector3.zero).UnscaledTime();
            yield return LerpEnumerator.LocalScale(rt_item, 0.25f, Vector3.zero).Curve(EasingCurves.EaseInBack).UnscaledTime();
            yield return LerpEnumerator.Alpha(CanvasGroup, 0.5f, 0f).UnscaledTime();
            OnSubmit?.Invoke();
            Close(0);
        }
    }

    public void SetSprite(Sprite sprite)
    {
        img_item.sprite = sprite;
    }

    public void SetTitle(string title)
    {
        tmp_title.text = title;
    }

    public Coroutine Animate()
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            cvg_fx.alpha = 1;

            SoundController.Instance.Play(SoundEffectType.sfx_ui_tally);
            yield return LerpEnumerator.LocalScale(rt_glow, 0.15f, Vector3.one, Vector3.zero).UnscaledTime();
            SoundController.Instance.Play(SoundEffectType.sfx_ui_tally);
            yield return LerpEnumerator.LocalScale(rt_glow, 0.15f, Vector3.one, Vector3.zero).UnscaledTime();
            yield return new WaitForSecondsRealtime(0.2f);
            SoundController.Instance.Play(SoundEffectType.sfx_ui_unlock_ability);
            Lerp.Alpha(cvg_title, 0.5f, 1f).UnscaledTime();
            Lerp.LocalScale(rt_rays, 0.75f, Vector3.one).UnscaledTime();
            yield return LerpEnumerator.LocalScale(rt_item, 0.5f, Vector3.one).Curve(EasingCurves.EaseOutBack).UnscaledTime();
            Lerp.Alpha(input.CanvasGroup, 0.5f, 1f).UnscaledTime();
            animating = false;
        }
    }

    private void AnimateRaysRotation()
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var end = new Vector3(0, 0, 360f);
            while (true)
            {
                rt_rays.localEulerAngles = Vector3.zero;
                yield return LerpEnumerator.LocalEuler(rt_rays, 8f, end).UnscaledTime();
            }
        }
    }
}