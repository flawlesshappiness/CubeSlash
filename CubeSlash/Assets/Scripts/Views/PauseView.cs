using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseView : View
{
    public static bool Exists { get; private set; }

    [SerializeField] private SelectableMenuItem btnContinue;
    [SerializeField] private SelectableMenuItem btnOptions;
    [SerializeField] private SelectableMenuItem btnMainMenu;

    private void OnEnable()
    {
        Exists = true;
        GameController.Instance.PauseLock.AddLock(nameof(PauseView));
    }

    private void OnDisable()
    {
        Exists = false;
        GameController.Instance.PauseLock.RemoveLock(nameof(PauseView));

        PlayerInput.Controls.Player.Menu.started -= Menu_started;
    }

    private void Start()
    {
        PlayerInput.Controls.Player.Menu.started += Menu_started;

        btnContinue.onSubmit += ClickContinue;
        btnOptions.onSubmit += ClickOptions;
        btnMainMenu.onSubmit += ClickMainMenu;
    }

    IEnumerator TransitionToOptionsCr()
    {
        Interactable = false;
        SelectableMenuItem.RemoveSelection();

        var lerp = LerpEnumerator.Value(0.5f, f =>
        {
            CanvasGroup.alpha = Mathf.Lerp(1f, 0f, f);
        });
        lerp.UseUnscaledTime = true;
        yield return lerp;

        var view = ViewController.Instance.ShowView<OptionsView>(0, nameof(OptionsView));
        view.onClickBack += () => ViewController.Instance.ShowView<PauseView>(0.5f, nameof(PauseView));

        Close(0);
    }

    private void Menu_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        ClickContinue();
    }

    private void ClickContinue()
    {
        GameStateController.Instance.SetGameState(GameStateType.PLAYING);
        Close(0);
    }

    private void ClickOptions()
    {
        StartCoroutine(TransitionToOptionsCr());
    }

    private void ClickMainMenu()
    {
        Close(0);
        GameController.Instance.ReturnToMainMenu();
    }
}