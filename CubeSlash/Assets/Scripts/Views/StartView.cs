using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StartView : View
{
    [SerializeField] private SelectableMenuItem btn_play;
    [SerializeField] private SelectableMenuItem btn_options;
    [SerializeField] private SelectableMenuItem btn_quit;

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
    }

    private void ClickPlay()
    {
        StartCoroutine(TransitionToBodySelectCr());
    }

    private void ClickOptions()
    {
        StartCoroutine(TransitionToOptions());
    }

    private void ClickQuit()
    {
        GameController.Instance.Quit();
    }

    IEnumerator TransitionToBodySelectCr()
    {
        Interactable = false;
        SelectableMenuItem.RemoveSelection();

        yield return LerpEnumerator.Value(0.5f, f =>
        {
            CanvasGroup.alpha = Mathf.Lerp(1f, 0f, f);
        });

        ViewController.Instance.ShowView<GameSetupView>(0.5f);
    }

    IEnumerator TransitionToOptions()
    {
        Interactable = false;
        SelectableMenuItem.RemoveSelection();

        yield return LerpEnumerator.Value(0.5f, f =>
        {
            CanvasGroup.alpha = Mathf.Lerp(1f, 0f, f);
        });

        var view = ViewController.Instance.ShowView<OptionsView>(0);
        view.onClickBack += () => ViewController.Instance.ShowView<StartView>(0.5f);

        Close(0);
    }
}