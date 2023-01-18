using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartView : View
{
    [SerializeField] private ButtonExtended btn_play;
    [SerializeField] private ButtonExtended btn_options;
    [SerializeField] private ButtonExtended btn_quit;

    private void Start()
    {
        btn_play.onClick.AddListener(ClickPlay);
        btn_options.onClick.AddListener(ClickOptions);
        btn_quit.onClick.AddListener(ClickQuit);

        EventSystemController.Instance.SetDefaultSelection(btn_play.gameObject);
        EventSystemController.Instance.EventSystem.SetSelectedGameObject(null);

        var first_level = LevelDatabase.Instance.levels[0];
        BackgroundController.Instance.FadeToLevel(first_level);
        VignetteController.Instance.SetLevel(first_level);

        btn_play.SetSelectOnHover(true);
        btn_options.SetSelectOnHover(true);
        btn_quit.SetSelectOnHover(true);

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
        FMODButtonEvent.PreviousSelected = null;
        //GameController.Instance.StartGame();
        ViewController.Instance.ShowView<BodySelectView>(0);
    }

    private void ClickOptions()
    {
        FMODButtonEvent.PreviousSelected = null;
        var view = ViewController.Instance.ShowView<OptionsView>(0);
        view.onClickBack += () => ViewController.Instance.ShowView<StartView>(0);
    }

    private void ClickQuit()
    {
        GameController.Instance.Quit();
    }
}