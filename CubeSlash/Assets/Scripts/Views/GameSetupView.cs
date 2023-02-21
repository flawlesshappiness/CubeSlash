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

    private bool transitioning;

    private void Start()
    {
        btn_play.onSubmit += ClickPlay;
        ability_panel.onSettingsChanged += OnAbilitySettingsChanged;

        input.AddInput(PlayerInput.UIButtonType.EAST, "Back");
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
        transitioning = false;
        Interactable = false;
        SelectableMenuItem.RemoveSelection();
        StartCoroutine(TransitionToGameCr());
    }

    private void PressBack(InputAction.CallbackContext context)
    {
        if (transitioning) return;
        transitioning = true;

        Interactable = false;
        SelectableMenuItem.RemoveSelection();
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            SoundController.Instance.Play(SoundEffectType.sfx_ui_submit);
            yield return LerpEnumerator.Alpha(CanvasGroup, 0.5f, 0f).UnscaledTime();
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

    IEnumerator TransitionToGameCr()
    {
        yield return LerpEnumerator.Alpha(CanvasGroup, 1f, 0f).UnscaledTime();

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