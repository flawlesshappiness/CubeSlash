using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashView : View
{
    [SerializeField] private List<CanvasGroup> cvg_splashes;

    private const float FADE_DURATION = 2f;
    private const float SHOW_DURATION = 3f;

    private bool skip_pressed;

    private void OnEnable()
    {
        PlayerInput.Controls.Player.South.started += SouthPressed;
    }

    private void OnDisable()
    {
        PlayerInput.Controls.Player.South.started -= SouthPressed;
    }

    private void SouthPressed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        skip_pressed = true;
    }

    public Coroutine ShowSplashes()
    {
        cvg_splashes.ForEach(cvg => cvg.alpha = 0);
        cvg_splashes.ForEach(cvg => cvg.gameObject.SetActive(true));
        return StartCoroutine(ShowSplashesCr());
    }

    private IEnumerator ShowSplashesCr()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        foreach (var cvg in cvg_splashes)
        {
            yield return SkippableLoopCr(FADE_DURATION, f => cvg.alpha = Mathf.Lerp(0, 1, f));
            yield return SkippableLoopCr(SHOW_DURATION, null);
            yield return SkippableLoopCr(FADE_DURATION, f => cvg.alpha = Mathf.Lerp(1, 0, f));
            cvg.alpha = 0;
            skip_pressed = false;
        }
    }

    private IEnumerator SkippableLoopCr(float duration, System.Action<float> onLoop)
    {
        var skipped = false;
        var time_start = Time.unscaledTime;
        var time_end = time_start + duration;
        while (Time.unscaledTime < time_end)
        {
            var t = (Time.unscaledTime - time_start) / (time_end - time_start);
            onLoop?.Invoke(t);

            if (!skipped && skip_pressed)
            {
                skipped = true;
                var mul = 0.1f;
                var dur_start = duration * mul * t;
                var dur_end = duration * mul * (1f - t);
                time_start = Time.unscaledTime - dur_start;
                time_end = Time.unscaledTime + dur_end;
            }

            yield return null;
        }
    }
}