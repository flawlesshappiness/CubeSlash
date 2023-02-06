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
        btn_play.onClick += ClickPlay;
        btn_options.onClick += ClickOptions;
        btn_quit.onClick += ClickQuit;

        EventSystemController.Instance.SetDefaultSelection(btn_play.gameObject);
        EventSystemController.Instance.EventSystem.SetSelectedGameObject(null);

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
        EventSystemController.Instance.SetDefaultSelection(null);
        EventSystemController.Instance.EventSystem.SetSelectedGameObject(null);

        yield return LerpEnumerator.Value(0.5f, f =>
        {
            CanvasGroup.alpha = Mathf.Lerp(1f, 0f, f);
        });

        FMODButtonEvent.PreviousSelected = null;
        //ViewController.Instance.ShowView<BodySelectView>(0);
        ViewController.Instance.ShowView<GameSetupView>(0);
    }

    IEnumerator TransitionToOptions()
    {
        Interactable = false;
        EventSystemController.Instance.SetDefaultSelection(null);
        EventSystemController.Instance.EventSystem.SetSelectedGameObject(null);

        yield return LerpEnumerator.Value(0.5f, f =>
        {
            CanvasGroup.alpha = Mathf.Lerp(1f, 0f, f);
        });

        FMODButtonEvent.PreviousSelected = null;
        var view = ViewController.Instance.ShowView<OptionsView>(0);
        view.onClickBack += () => ViewController.Instance.ShowView<StartView>(0.5f);

        Close(0);
    }
}