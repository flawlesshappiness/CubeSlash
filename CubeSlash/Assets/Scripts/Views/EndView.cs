using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EndView : View
{
    [SerializeField] private Image img_hold;
    [SerializeField] private UIInputLayout input;
    [SerializeField] private TMP_Text tmp_text_level, tmp_text_enemies, tmp_text_currency;
    [SerializeField] private TMP_Text tmp_value_level, tmp_value_enemies, tmp_value_currency;
    [SerializeField] private FMODEventReference sfx_stats_row;
    [SerializeField] private FMODEventReference sfx_tally;

    private Coroutine cr_animate_stats;
    private bool animating;
    private bool exiting;

    private bool InputDisabled { get { return animating || exiting; } }

    private void Start()
    {
        img_hold.SetAlpha(0);
        SetupInput();

        tmp_text_level.SetAlpha(0);
        tmp_text_enemies.SetAlpha(0);
        tmp_text_currency.SetAlpha(0);
        tmp_value_level.SetAlpha(0);
        tmp_value_enemies.SetAlpha(0);
        tmp_value_currency.SetAlpha(0);

        cr_animate_stats = StartCoroutine(AnimateStatsCr());
    }

    private void OnEnable()
    {
        GameController.Instance.PauseLock.AddLock(nameof(EndView));

        PlayerInput.Controls.Player.South.started += PressReturn;
        PlayerInput.Controls.Player.West.started += PressPlayEndless;
    }

    private void OnDisable()
    {
        GameController.Instance.PauseLock.RemoveLock(nameof(EndView));

        PlayerInput.Controls.Player.South.started -= PressReturn;
        PlayerInput.Controls.Player.West.started -= PressPlayEndless;
    }

    private void SetupInput()
    {
        input.AddInput(PlayerInput.UIButtonType.SOUTH, "Hold to return to menu");
        input.AddInput(PlayerInput.UIButtonType.WEST, "Hold to play endless");
    }

    IEnumerator AnimateStatsCr()
    {
        animating = true;
        yield return new WaitForSecondsRealtime(1f);
        yield return RowCr(tmp_text_level, tmp_value_level, 9);
        yield return new WaitForSecondsRealtime(0.2f);
        yield return RowCr(tmp_text_enemies, tmp_value_enemies, 99);
        yield return new WaitForSecondsRealtime(0.3f);
        yield return RowCr(tmp_text_currency, tmp_value_currency, 999);
        animating = false;

        IEnumerator RowCr(TMP_Text tmp_text, TMP_Text tmp_value, int value)
        {
            sfx_stats_row.Play();
            tmp_text.SetAlpha(1);
            tmp_value.SetAlpha(1);
            yield return TallyPointsCr(0.5f, tmp_value, value);
        }

        IEnumerator TallyPointsCr(float duration, TMP_Text text, int value)
        {
            yield return LerpEnumerator.Value(duration, f =>
            {
                var v = (int)Mathf.Lerp(0, value, f);
                text.text = v.ToString();
            }).UnscaledTime().Curve(EasingCurves.EaseOutQuad);
        }
    }

    private void SkipAnimateStats()
    {
        StopCoroutine(cr_animate_stats);
        tmp_text_level.SetAlpha(1);
        tmp_text_enemies.SetAlpha(1);
        tmp_text_currency.SetAlpha(1);
        tmp_value_level.SetAlpha(1);
        tmp_value_enemies.SetAlpha(1);
        tmp_value_currency.SetAlpha(1);
        animating = false;
    }

    private void ReturnToMainMenu()
    {
        exiting = true;
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            GameController.Instance.EndGame();
            var fg_view = ViewController.Instance.ShowView<ForegroundView>(1, "Foreground");
            yield return new WaitForSecondsRealtime(1);
            GameController.Instance.MainMenu();
            yield return new WaitForSecondsRealtime(0.5f);
            fg_view.Close(1f);
            Close(0);
        }
    }

    private void PlayEndless()
    {
        Close(0);
        GameController.Instance.SetTimeScale(1f);
        GameStateController.Instance.SetGameState(GameStateType.PLAYING);
    }

    private void PressReturn(InputAction.CallbackContext c)
    {
        if (animating)
        {
            SkipAnimateStats();
        }
        else if (InputDisabled)
        {
            // Do nothing
        }
        else
        {
            ReturnToMainMenu();
            sfx_stats_row.Play();
        }
    }

    private void PressPlayEndless(InputAction.CallbackContext c)
    {
        if (animating)
        {
            SkipAnimateStats();
        }
        else if (InputDisabled)
        {
            // Do nothing
        }
        else
        {
            PlayEndless();
            sfx_stats_row.Play();
        }
    }
}