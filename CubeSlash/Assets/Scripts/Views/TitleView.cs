using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleView : View
{
    [SerializeField] private UIInputLayout input;

    private bool transitioning = true;

    private void Start()
    {
        input.Clear();
        input.AddInput(PlayerInput.UIButtonType.SOUTH, "Start");

        PlayerInput.Controls.UI.Submit.started += c => ClickStart();

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSecondsRealtime(2f);
            transitioning = false;
        }
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