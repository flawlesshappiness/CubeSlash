using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsView : View
{
    [SerializeField] private UISlider slider_master, slider_music, slider_sfx;
    [SerializeField] private ButtonExtended btn_back;

    public System.Action onClickBack;

    private void Start()
    {
        slider_master.SetValue(Save.Game.volume_master);
        slider_music.SetValue(Save.Game.volume_music);
        slider_sfx.SetValue(Save.Game.volume_sfx);

        slider_master.onValueChanged += AdjustMasterVolume;
        slider_music.onValueChanged += AdjustMusicVolume;
        slider_sfx.onValueChanged += AdjustSFXVolume;

        btn_back.onClick.AddListener(ClickBack);

        slider_master.btn.SetSelectOnHover(true);
        slider_music.btn.SetSelectOnHover(true);
        slider_sfx.btn.SetSelectOnHover(true);
        btn_back.SetSelectOnHover(true);

        EventSystemController.Instance.SetDefaultSelection(btn_back.gameObject);
        EventSystemController.Instance.EventSystem.SetSelectedGameObject(null);

        StartCoroutine(TransitionShowCr(true));
    }

    private IEnumerator TransitionShowCr(bool show)
    {
        var start = show ? 0f : 1f;
        var end = show ? 1f : 0f;
        CanvasGroup.alpha = start;

        var lerp = LerpEnumerator.Value(0.5f, f =>
        {
            CanvasGroup.alpha = Mathf.Lerp(start, end, f);
        });
        lerp.UnscaledTime = true;
        yield return lerp;
    }

    private IEnumerator TransitionBackCr()
    {
        yield return TransitionShowCr(false);
        Close(0);
        onClickBack();
    }

    private void OnEnable()
    {
        GameController.Instance.PauseLock.AddLock(nameof(OptionsView));
    }

    private void OnDisable()
    {
        GameController.Instance.PauseLock.RemoveLock(nameof(OptionsView));
    }

    private void AdjustMasterVolume()
    {
        var value = slider_master.GetPercentage();
        AudioController.Instance.SetMasterVolume(value);
        Save.Game.volume_master = value;
    }

    private void AdjustMusicVolume()
    {
        var value = slider_music.GetPercentage();
        AudioController.Instance.SetMusicVolume(value);
        Save.Game.volume_music = value;
    }

    private void AdjustSFXVolume()
    {
        var value = slider_sfx.GetPercentage();
        AudioController.Instance.SetSFXVolume(value);
        Save.Game.volume_sfx = value;
    }

    private void ClickBack()
    {
        StartCoroutine(TransitionBackCr());
    }
}