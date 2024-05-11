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

    private bool initiated;
    private float time_started;

    private void OnEnable()
    {
        Exists = true;
        GameController.Instance.PauseLock.AddLock(nameof(PauseView));

        PlayerInputController.Instance.Pause.Pressed += PressPause;
    }

    private void OnDisable()
    {
        Exists = false;
        GameController.Instance.PauseLock.RemoveLock(nameof(PauseView));

        PlayerInputController.Instance.Pause.Pressed -= PressPause;
    }

    private void Start()
    {
        btnContinue.onSubmit += ClickContinue;
        btnOptions.onSubmit += ClickOptions;
        btnMainMenu.onSubmit += ClickMainMenu;
        btnEndRun.onSubmit += ClickEndRun;

        time_started = Time.unscaledTime;
        initiated = true;
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

    private void PressPause()
    {
        return;
        if (!initiated) return;
        if (Time.unscaledTime < time_started + 0.2f) return;

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