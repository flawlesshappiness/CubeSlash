using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StartView : View
{
    [SerializeField] private MenuButton btn_play;
    [SerializeField] private MenuButton btn_options;
    [SerializeField] private MenuButton btn_quit;

    private void OnEnable()
    {
        GameStateController.Instance.SetGameState(GameStateType.MENU);
    }

    private void Start()
    {
        btn_play.Button.onClick.AddListener(ClickPlay);
        btn_options.Button.onClick.AddListener(ClickOptions);
        btn_quit.Button.onClick.AddListener(ClickQuit);

        EventSystemController.Instance.SetDefaultSelection(btn_play.Button.gameObject);
        EventSystemController.Instance.EventSystem.SetSelectedGameObject(null);

        BackgroundController.Instance.FadeToArea(GameSettings.Instance.main_menu_area);
        VignetteController.Instance.SetArea(GameSettings.Instance.main_menu_area);
        CameraController.Instance.AnimateSize(2f, 15f, EasingCurves.EaseInOutQuad);

        btn_play.Button.SetSelectOnHover(true);
        btn_options.Button.SetSelectOnHover(true);
        btn_quit.Button.SetSelectOnHover(true);

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.interactable = false;
            yield return new WaitForSecondsRealtime(0.5f);
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.interactable = true;
        }
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
        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.interactable = false;
        EventSystemController.Instance.SetDefaultSelection(null);
        EventSystemController.Instance.EventSystem.SetSelectedGameObject(null);
        btn_play.OnSelectionChanged(false);

        StartCoroutine(FadeButtonCr(btn_options));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(FadeButtonCr(btn_quit));
        yield return new WaitForSeconds(0.25f);

        yield return LerpEnumerator.Value(0.5f, f =>
        {
            CanvasGroup.alpha = Mathf.Lerp(1f, 0f, f);
        });

        FMODButtonEvent.PreviousSelected = null;
        ViewController.Instance.ShowView<BodySelectView>(0);
    }

    IEnumerator TransitionToOptions()
    {
        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.interactable = false;
        EventSystemController.Instance.SetDefaultSelection(null);
        EventSystemController.Instance.EventSystem.SetSelectedGameObject(null);
        btn_play.OnSelectionChanged(false);

        yield return LerpEnumerator.Value(0.5f, f =>
        {
            CanvasGroup.alpha = Mathf.Lerp(1f, 0f, f);
        });

        FMODButtonEvent.PreviousSelected = null;
        var view = ViewController.Instance.ShowView<OptionsView>(0);
        view.onClickBack += () => ViewController.Instance.ShowView<StartView>(0.5f);

        Close(0);
    }

    IEnumerator FadeButtonCr(MenuButton btn)
    {
        var start = btn.CanvasGroup.alpha;
        yield return LerpEnumerator.Value(0.25f, f =>
        {
            btn.CanvasGroup.alpha = Mathf.Lerp(start, 0f, f);
        });
    }
}