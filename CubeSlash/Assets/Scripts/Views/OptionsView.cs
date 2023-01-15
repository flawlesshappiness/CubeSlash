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
    }

    private void AdjustMasterVolume()
    {
        var value = slider_master.GetPercentage();
        FMODController.Instance.SetMasterVolume(value);
        Save.Game.volume_master = value;
    }

    private void AdjustMusicVolume()
    {
        var value = slider_music.GetPercentage();
        FMODController.Instance.SetMusicVolume(value);
        Save.Game.volume_music = value;
    }

    private void AdjustSFXVolume()
    {
        var value = slider_sfx.GetPercentage();
        FMODController.Instance.SetSFXVolume(value);
        Save.Game.volume_sfx = value;
    }

    private void ClickBack()
    {
        Close(0);
        onClickBack();
    }
}