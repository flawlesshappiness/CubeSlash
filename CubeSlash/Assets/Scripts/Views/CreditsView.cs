using Flawliz.GenericOptions;
using Flawliz.Lerp;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CreditsView : View
{
    [SerializeField] private ButtonControl _btn_back, _btn_ewpho;

    private bool _animating;

    private void Start()
    {
        _btn_back.OnSubmitEvent += ClickBack;
        _btn_ewpho.OnSubmitEvent += ClickEwpho;

        EventSystem.current.SetSelectedGameObject(_btn_back.gameObject);
    }

    private void ClickBack()
    {
        if (_animating) return;
        SoundController.Instance.Play(SoundEffectType.sfx_ui_submit);
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            _animating = true;
            yield return LerpEnumerator.Alpha(CanvasGroup, 0.5f, 0f);
            ViewController.Instance.ShowView<PlayRadialView>(0);
        }
    }

    private void ClickEwpho()
    {
        if (_animating) return;
        Application.OpenURL("https://soundcloud.com/ewpho");
        SoundController.Instance.Play(SoundEffectType.sfx_ui_submit);
    }
}