using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartView : View
{
    [SerializeField] private SelectableMenuItem btn_play;
    [SerializeField] private SelectableMenuItem btn_options;
    [SerializeField] private SelectableMenuItem btn_quit;
    [SerializeField] private RadialMenu radial_menu;

    private void OnEnable()
    {
        GameStateController.Instance.SetGameState(GameStateType.MENU);
    }

    private void Start()
    {
        btn_play.onSubmit += ClickPlay;
        btn_options.onSubmit += ClickOptions;
        btn_quit.onSubmit += ClickQuit;

        BackgroundController.Instance.FadeToArea(GameSettings.Instance.main_menu_area);
        VignetteController.Instance.SetArea(GameSettings.Instance.main_menu_area);
        CameraController.Instance.AnimateSize(2f, 15f, EasingCurves.EaseInOutQuad);

        radial_menu.AddOptions(new List<RadialMenuOption>
        {
            new RadialMenuOption
            {
                Title = "Play",
                Sprite = Icon.Get(IconType.start_game),
                OnSubmit = ClickPlay
            },
            new RadialMenuOption
            {
                Title = "Options",
                Sprite = Icon.Get(IconType.settings),
                OnSubmit = ClickOptions
            },
            new RadialMenuOption
            {
                Title = "Quit",
                Sprite = Icon.Get(IconType.quit),
                OnSubmit = ClickQuit
            },
            new RadialMenuOption
            {
                Title = "Customize",
                Sprite = Icon.Get(IconType.customize_body),
                OnSubmit = ClickCustomizeBody
            },
        });

        StartCoroutine(TransitionShowCr(true));
    }

    private void ClickPlay()
    {
        StartCoroutine(TransitionToGameSetupCr());
    }

    private void ClickOptions()
    {
        StartCoroutine(TransitionToOptions());
    }

    private void ClickQuit()
    {
        GameController.Instance.Quit();
    }

    private void ClickCustomizeBody()
    {
        ViewController.Instance.ShowView<BodyEditView>();
    }

    IEnumerator TransitionToGameSetupCr()
    {
        SelectableMenuItem.RemoveSelection();
        yield return TransitionShowCr(false);
        ViewController.Instance.ShowView<GameSetupView>(0);
    }

    IEnumerator TransitionToOptions()
    {
        SelectableMenuItem.RemoveSelection();
        yield return TransitionShowCr(false);

        var view = ViewController.Instance.ShowView<OptionsView>(0);
        view.onClickBack += () => ViewController.Instance.ShowView<StartView>(0);

        Close(0);
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
}