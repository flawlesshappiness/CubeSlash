using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class TitleView : View
{
    private bool transitioning = true;

    private void Start()
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSecondsRealtime(2f);
            transitioning = false;
        }
    }

    private void OnEnable()
    {
        PlayerInputController.Instance.Submit.Pressed += ClickStart;
    }

    private void OnDisable()
    {
        PlayerInputController.Instance.Submit.Pressed -= ClickStart;
    }

    private void ClickStart()
    {
        if (transitioning) return;
        transitioning = true;

        SoundController.Instance.Play(SoundEffectType.sfx_ui_marima_001);

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return LerpEnumerator.Alpha(CanvasGroup, 0.25f, 0f);
            ViewController.Instance.ShowView<StartView>(0);
        }
    }
}