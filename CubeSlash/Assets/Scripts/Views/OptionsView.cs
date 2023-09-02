using Flawliz.GenericOptions;
using Flawliz.Lerp;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class OptionsView : View
{
    [SerializeField] private UISlider slider_master, slider_music, slider_sfx;
    [SerializeField] private SelectableMenuItem btn_delete_save;
    [SerializeField] private UIInputLayout input;
    [SerializeField] private GenericOptions options;

    public System.Action onClickBack;

    private void Start()
    {
        /*
        slider_master.SetValue(Save.Game.volumes[FMODBusType.Master]);
        slider_music.SetValue(Save.Game.volumes[FMODBusType.Music]);
        slider_sfx.SetValue(Save.Game.volumes[FMODBusType.SFX]);
        */

        slider_master.onValueChanged += AdjustMasterVolume;
        slider_music.onValueChanged += AdjustMusicVolume;
        slider_sfx.onValueChanged += AdjustSFXVolume;

        btn_delete_save.onSubmit += ClickDeleteSave;

        options.OnBack += Back;

        StartCoroutine(TransitionShowCr(true));

        input.AddInput(PlayerInput.UIButtonType.EAST, "Back");
    }

    private IEnumerator TransitionShowCr(bool show)
    {
        Interactable = false;
        var start = show ? 0f : 1f;
        var end = show ? 1f : 0f;
        CanvasGroup.alpha = start;
        yield return LerpEnumerator.Alpha(CanvasGroup, 0.2f, end).UnscaledTime();
        Interactable = show;
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

        PlayerInput.Controls.Player.East.started += PressBack;
    }

    private void OnDisable()
    {
        GameController.Instance.PauseLock.RemoveLock(nameof(OptionsView));

        PlayerInput.Controls.Player.East.started -= PressBack;
    }

    private void PressBack(InputAction.CallbackContext context)
    {
        if (!Interactable) return;
        options.ClickBack();
    }

    private void Back()
    {
        SelectableMenuItem.RemoveSelection();
        SoundController.Instance.Play(SoundEffectType.sfx_ui_submit);
        StartCoroutine(TransitionBackCr());
    }

    private void AdjustMasterVolume()
    {
        var value = slider_master.GetPercentage();
        AudioController.Instance.SetMasterVolume(value);
        //Save.Game.volumes[FMODBusType.Master] = value;
    }

    private void AdjustMusicVolume()
    {
        var value = slider_music.GetPercentage();
        AudioController.Instance.SetMusicVolume(value);
        //Save.Game.volumes[FMODBusType.Music] = value;
    }

    private void AdjustSFXVolume()
    {
        var value = slider_sfx.GetPercentage();
        AudioController.Instance.SetSFXVolume(value);
        AudioController.Instance.SetUIVolume(value);
        //Save.Game.volumes[FMODBusType.SFX] = value;
        //Save.Game.volumes[FMODBusType.UI] = value;
    }

    private void ClickDeleteSave()
    {
        var prev_selected = EventSystem.current.currentSelectedGameObject;
        Interactable = false;
        var view = ConfirmView.Show("Are you sure?\nThis will delete all saved progress.", Confirm, Back);
        view.SetWrongPanel();

        void Confirm()
        {
            SaveDataController.Instance.ClearSaveData();
            Player.Instance.Clear();
            Back();
            PressBack(new InputAction.CallbackContext());
        }

        void Back()
        {
            Interactable = true;
            EventSystem.current.SetSelectedGameObject(prev_selected);
        }
    }
}