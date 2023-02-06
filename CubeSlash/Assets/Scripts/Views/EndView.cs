using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EndView : View
{
    [SerializeField] private UIInputLayout input;
    [SerializeField] private TMP_Text tmp_title_win, tmp_title_lose;
    [SerializeField] private CanvasGroup cvg_text_level, cvg_text_enemies, cvg_text_currency, cvg_text_time;
    [SerializeField] private TMP_Text tmp_value_level, tmp_value_enemies, tmp_value_currency, tmp_value_time;
    [SerializeField] private Image img_title_gradient;
    [SerializeField] private CanvasGroup cvg_title, cvg_stats, cvg_background;
    [SerializeField] private UICurrencyBar currencybar;
    [SerializeField] private FMODEventReference sfx_stats_row;
    [SerializeField] private FMODEventReference sfx_tally;

    private Coroutine cr_animate_stats;
    private bool starting;
    private bool animating_stats;
    private bool exiting;

    private int currency_earned;

    private bool InputDisabled { get { return starting || animating_stats || exiting; } }

    private void Start()
    {
        SetTextAlpha(0);
        SetupInput();

        currencybar.gameObject.SetActive(false);
        input.gameObject.SetActive(false);
        cvg_title.alpha = 0;
        cvg_stats.alpha = 0;
        cvg_background.alpha = 0;

        var data = SessionController.Instance.CurrentData;
        tmp_title_win.enabled = data.won;
        tmp_title_lose.enabled = !data.won;
        currency_earned = data.GetCurrencyEarned();
        CurrencyController.Instance.Gain(CurrencyType.DNA, currency_earned);
        data.has_received_currency = true;

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            starting = true;

            if (data.won) yield return AnimateWonCr();
            else yield return AnimateLoseCr();

            cr_animate_stats = StartCoroutine(AnimateStatsCr());
            yield return cr_animate_stats;

            starting = false;
        }
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
        input.AddInput(PlayerInput.UIButtonType.SOUTH, "Return to main menu");

        if (SessionController.Instance.CurrentData.won)
        {
            input.AddInput(PlayerInput.UIButtonType.WEST, "Continue playing (endless)");
        }
    }

    IEnumerator AnimateLoseCr()
    {
        img_title_gradient.enabled = true;
        yield return LerpEnumerator.Value(3f, f =>
        {
            cvg_title.alpha = Mathf.Lerp(0, 1, f);
        }).UnscaledTime();

        yield return LerpEnumerator.Value(1f, f =>
        {
            img_title_gradient.SetAlpha(Mathf.Lerp(1, 0, f));
            cvg_background.alpha = Mathf.Lerp(0, 1, f);
            cvg_stats.alpha = Mathf.Lerp(0, 1, f);

        }).UnscaledTime();

        yield return new WaitForSecondsRealtime(1f);
    }

    IEnumerator AnimateWonCr()
    {
        img_title_gradient.enabled = false;
        yield return LerpEnumerator.Value(1f, f =>
        {
            cvg_background.alpha = Mathf.Lerp(0, 1, f);
            cvg_title.alpha = Mathf.Lerp(0, 1, f);
            cvg_stats.alpha = Mathf.Lerp(0, 1, f);
        }).UnscaledTime();
        yield return new WaitForSecondsRealtime(1f);
    }

    IEnumerator AnimateStatsCr()
    {
        animating_stats = true;
        var data = SessionController.Instance.CurrentData;
        var lifetime = Time.time - data.time_start;
        yield return RowCr(cvg_text_level, tmp_value_level, data.levels_gained);
        yield return new WaitForSecondsRealtime(0.2f);
        yield return RowCr(cvg_text_time, tmp_value_time, (int)lifetime);
        yield return new WaitForSecondsRealtime(0.2f);
        yield return RowCr(cvg_text_enemies, tmp_value_enemies, data.enemies_killed);
        yield return new WaitForSecondsRealtime(0.5f);
        currencybar.gameObject.SetActive(true);
        var prev_currency = Currency.DNA - currency_earned;
        currencybar.SetValueText(prev_currency);
        currencybar.AnimateUpdateValue(0.5f, EasingCurves.EaseOutQuad);
        yield return RowCr(cvg_text_currency, tmp_value_currency, currency_earned);
        input.gameObject.SetActive(true);
        animating_stats = false;

        IEnumerator RowCr(CanvasGroup cvg_text, TMP_Text tmp_value, int value)
        {
            sfx_stats_row.Play();
            cvg_text.alpha = 1;
            tmp_value.SetAlpha(1);
            yield return TallyPointsCr(0.5f, tmp_value, value);
        }

        IEnumerator TallyPointsCr(float duration, TMP_Text text, int value)
        {
            var i_last = 0;
            yield return LerpEnumerator.Value(duration, f =>
            {
                var v = (int)Mathf.Lerp(0, value, f);
                text.text = v.ToString();

                if(v > i_last)
                {
                    i_last = v;
                    FMODController.Instance.PlayWithLimitDelay(sfx_tally);
                }
            }).UnscaledTime().Curve(EasingCurves.EaseOutQuad);
        }
    }

    private void SkipAnimateStats()
    {
        StopCoroutine(cr_animate_stats);
        SetTextAlpha(1);
        animating_stats = false;
    }

    private void SetTextAlpha(float a)
    {
        SetTextAlpha(a, 
            tmp_value_level, 
            tmp_value_time, 
            tmp_value_enemies, 
            tmp_value_currency
            );

        SetCanvasGroupAlpha(a,
            cvg_text_level,
            cvg_text_time,
            cvg_text_enemies,
            cvg_text_currency
            );
    }

    private void SetTextAlpha(float alpha, params TMP_Text[] tmps)
    {
        foreach(var tmp in tmps)
        {
            tmp.SetAlpha(alpha);
        }
    }

    private void SetCanvasGroupAlpha(float alpha, params CanvasGroup[] cvgs)
    {
        foreach (var cvg in cvgs)
        {
            cvg.alpha = alpha;
        }
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
        GameController.Instance.ResumeEndless();
    }

    private void PressReturn(InputAction.CallbackContext c)
    {
        if (animating_stats)
        {
            SkipAnimateStats();
        }
        else if (!InputDisabled)
        {
            ReturnToMainMenu();
            sfx_stats_row.Play();
        }
    }

    private void PressPlayEndless(InputAction.CallbackContext c)
    {
        if (animating_stats)
        {
            SkipAnimateStats();
        }
        else if (!InputDisabled)
        {
            PlayEndless();
            sfx_stats_row.Play();
        }
    }
}