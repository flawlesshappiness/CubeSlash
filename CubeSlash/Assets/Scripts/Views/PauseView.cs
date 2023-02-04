using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseView : View
{
    public static bool Exists { get; private set; }

    [SerializeField] private ButtonExtended btnContinue;
    [SerializeField] private ButtonExtended btnOptions;
    [SerializeField] private ButtonExtended btnMainMenu;

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

        btnContinue.onClick.AddListener(ClickContinue);
        btnOptions.onClick.AddListener(ClickOptions);
        btnMainMenu.onClick.AddListener(ClickMainMenu);

        EventSystemController.Instance.SetDefaultSelection(btnContinue.gameObject);
        EventSystemController.Instance.EventSystem.SetSelectedGameObject(null);

        btnContinue.SetSelectOnHover(true);
        btnOptions.SetSelectOnHover(true);
        btnMainMenu.SetSelectOnHover(true);
    }

    IEnumerator TransitionToOptionsCr()
    {
        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.interactable = false;
        EventSystemController.Instance.SetDefaultSelection(null);
        EventSystemController.Instance.EventSystem.SetSelectedGameObject(null);

        var lerp = LerpEnumerator.Value(0.5f, f =>
        {
            CanvasGroup.alpha = Mathf.Lerp(1f, 0f, f);
        });
        lerp.UseUnscaledTime = true;
        yield return lerp;

        FMODButtonEvent.PreviousSelected = null;
        var view = ViewController.Instance.ShowView<OptionsView>(0);
        view.onClickBack += () => ViewController.Instance.ShowView<PauseView>(0.5f);

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