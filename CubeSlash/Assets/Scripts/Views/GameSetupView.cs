using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameSetupView : View
{
    [SerializeField] private UIAbilityPanel ability_panel;
    [SerializeField] private UICharmPanel charm_panel;
    [SerializeField] private UIDifficultyPanel difficulty_panel;
    [SerializeField] private SelectableMenuItem btn_play;
    [SerializeField] private UIInputLayout input;

    private void Start()
    {
        btn_play.onSubmit += ClickPlay;
        ability_panel.onSettingsChanged += OnAbilitySettingsChanged;

        input.AddInput(PlayerInput.UIButtonType.EAST, "Back");

        StartCoroutine(TransitionShowCr(true));
    }

    private void OnEnable()
    {
        PlayerInput.Controls.Player.East.started += PressBack;
    }

    private void OnDisable()
    {
        PlayerInput.Controls.Player.East.started -= PressBack;
    }

    private void ClickPlay()
    {
        Interactable = false;
        SelectableMenuItem.RemoveSelection();
        StartCoroutine(TransitionToGameCr());
    }

    private void PressBack(InputAction.CallbackContext context)
    {
        if (!Interactable) return;

        Interactable = false;
        SelectableMenuItem.RemoveSelection();
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            SoundController.Instance.Play(SoundEffectType.sfx_ui_submit);
            yield return TransitionShowCr(false);
            ViewController.Instance.ShowView<StartView>(0.5f);
            Close(0);
        }
    }

    private void OnAbilitySettingsChanged()
    {
        var unlocked = ability_panel.CurrentSettings.IsUnlocked();
        var a = unlocked ? 1 : 0.25f;
        Lerp.Alpha(btn_play.CanvasGroup, 0.25f, a);
        btn_play.interactable = unlocked;
    }

    private IEnumerator TransitionShowCr(bool show)
    {
        Interactable = false;
        var start = show ? 0f : 1f;
        var end = show ? 1f : 0f;
        CanvasGroup.alpha = start;
        yield return LerpEnumerator.Alpha(CanvasGroup, 0.5f, end).UnscaledTime();
        Interactable = show;
    }

    IEnumerator TransitionToGameCr()
    {
        // Show intro
        Interactable = false;
        var view_intro = ViewController.Instance.ShowView<GameIntroView>(0.5f, nameof(GameIntroView));
        MusicController.Instance.FadeOutBGM(2f);
        yield return new WaitForSecondsRealtime(1f);
        yield return view_intro.AnimateIntro();
        view_intro.Close(1f);

        // Set difficulty
        DifficultyController.Instance.SetDifficulty(difficulty_panel.SelectedIndex);

        // Setup player
        Player.Instance.gameObject.SetActive(true);
        Player.Instance.Clear();

        var charms = charm_panel.GetActivatedCharms();
        charms.ForEach(c => UpgradeController.Instance.UnlockUpgrade(c.upgrade_id));

        Player.Instance.SetPlayerBody(ability_panel.CurrentSettings);

        // Start
        GameController.Instance.StartGame();
    }
}