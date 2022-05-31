using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelTransitionView : View
{
    [SerializeField] private Image img_timer;
    [SerializeField] private TMP_Text tmp_value_ap;

    private ILerp lerp_timer;

    private void Start()
    {
        // Text
        tmp_value_ap.text = Data.Game.count_ability_points.ToString();

        // Wait for input
        var cr_input = CoroutineController.Instance.Run(WaitForInputCr(), "timer_" + GetInstanceID());

        // Timer
        lerp_timer = Lerp.Value(2f, 1f, 0f, t =>
        {
            img_timer.fillAmount = t;
        }, gameObject, "timer_" + GetInstanceID())
            .Delay(1)
            .OnEnd(() =>
            {
                CoroutineController.Instance.Kill(cr_input);
                GameController.Instance.NextLevelTransition();
            });
    }

    private IEnumerator WaitForInputCr()
    {
        var input_received = false;
        while (!input_received)
        {
            if (PlayerInputController.Instance.GetAnyJoystickButtonDown())
            {
                input_received = true;
                Lerp.Kill(lerp_timer);
                GameController.Instance.AbilityMenuTransition();
            }
            yield return null;
        }
    }
}