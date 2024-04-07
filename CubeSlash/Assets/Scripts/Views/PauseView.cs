using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class PauseView : View
{
    public static bool Exists { get; private set; }

    [SerializeField] private SelectableMenuItem btnContinue;
    [SerializeField] private SelectableMenuItem btnOptions;
    [SerializeField] private SelectableMenuItem btnMainMenu;
    [SerializeField] private SelectableMenuItem btnEndRun;

    private void OnEnable()
    {
        Exists = true;
        GameController.Instance.PauseLock.AddLock(nameof(PauseView));

        PlayerInputController.Instance.Pause.Pressed += Menu_started;
    }

    private void OnDisable()
    {
        Exists = false;
        GameController.Instance.PauseLock.RemoveLock(nameof(PauseView));

        PlayerInputController.Instance.Pause.Pressed -= Menu_started;
    }

    private void Start()
    {
        btnContinue.onSubmit += ClickContinue;
        btnOptions.onSubmit += ClickOptions;
        btnMainMenu.onSubmit += ClickMainMenu;
        btnEndRun.onSubmit += ClickEndRun;
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

    private void Menu_started()
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

    private void ClickEndRun()
    {
        Close(0);
        Player.Instance.Suicide();
    }
}