using Flawliz.Lerp;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameIntroView : View
{
    [SerializeField] private UIInputLayout input;
    [SerializeField] private TMP_Text[] tmps_intro;
    private bool skip;

    private void Start()
    {
        foreach (var tmp in tmps_intro)
        {
            tmp.color = tmp.color.SetA(0);
        }

        input.CanvasGroup.alpha = 0;
        input.Clear();
        input.AddInput(PlayerInput.UIButtonType.SOUTH, "Skip");
    }

    private void OnEnable()
    {
        PlayerInput.Controls.Player.South.started += PressSkip;
    }

    private void OnDisable()
    {
        PlayerInput.Controls.Player.South.started -= PressSkip;
    }

    public Coroutine AnimateIntro()
    {
        var time_fade = 1f;
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            Lerp.Alpha(input.CanvasGroup, 0.5f, 1);
            foreach (var tmp in tmps_intro)
            {
                var words = tmp.text.Split(' ').Length;
                var time_read = words / 2;
                var time = Time.unscaledTime + time_read;
                Lerp.Alpha(tmp, time_fade, 1).UnscaledTime();
                while (!skip && Time.unscaledTime < time)
                {
                    yield return null;
                }

                skip = false;
            }
        }
    }

    private void PressSkip(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        skip = true;
    }
}