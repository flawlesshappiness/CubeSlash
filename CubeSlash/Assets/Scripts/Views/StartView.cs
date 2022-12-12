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
        btn_quit.onClick.AddListener(ClickQuit);

        EventSystemController.Instance.SetDefaultSelection(btn_play.gameObject);
        BackgroundController.Instance.FadeToLevel(LevelDatabase.Instance.levels[0], 5);

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
            EventSystemController.Instance.EventSystem.SetSelectedGameObject(null);

        }
    }

    private void ClickPlay()
    {
        FMODButtonEvent.PreviousSelected = null;
        GameController.Instance.StartGame();
    }

    private void ClickQuit()
    {
        Application.Quit();
    }
}