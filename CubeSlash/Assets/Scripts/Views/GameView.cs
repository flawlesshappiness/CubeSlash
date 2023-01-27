using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Flawliz.Lerp;

public class GameView : View
{
    [SerializeField] private Image img_experience;
    [SerializeField] private CanvasGroup cvg_tutorial;
    [SerializeField] private UIInputLayout input_tutorial;

    private void Start()
    {
        UpdateExperience(false);
        ShowIntroTutorial();
    }

    private void OnEnable()
    {
        Player.Instance.Experience.onValueChanged += OnExperienceChanged;
    }

    private void OnDisable()
    {
        Player.Instance.Experience.onValueChanged -= OnExperienceChanged;
    }

    private void OnDestroy()
    {
        Player.Instance.Experience.onValueChanged -= OnExperienceChanged;
    }

    private void OnExperienceChanged()
    {
        UpdateExperience(true);
    }

    private void UpdateExperience(bool animate)
    {
        var exp = Player.Instance.Experience;
        var t = (float)exp.Value / exp.Max;
        if (animate)
        {
            Lerp.Value("fill_" + img_experience.GetInstanceID(), 0.25f, img_experience.fillAmount, t, f => img_experience.fillAmount = f)
                .Connect(img_experience.gameObject)
                .Curve(EasingCurves.EaseOutQuad)
                .UnscaledTime();
        }
        else
        {
            img_experience.fillAmount = t;
        }
    }

    public void ShowIntroTutorial()
    {
        input_tutorial.AddInput(PlayerInput.UIButtonType.NAV_ALL, "Move");
        input_tutorial.AddInput(PlayerInput.UIButtonType.SOUTH, "Use equipped ability");
        input_tutorial.AddInput(PlayerInput.UIButtonType.EAST, "Use equipped ability");
        input_tutorial.AddInput(PlayerInput.UIButtonType.WEST, "Use equipped ability");
        input_tutorial.AddInput(PlayerInput.UIButtonType.NORTH, "Use equipped ability");

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return LerpEnumerator.Value(1f, f =>
            {
                cvg_tutorial.alpha = Mathf.Lerp(0f, 1f, f);
            });

            yield return new WaitForSeconds(8f);

            yield return LerpEnumerator.Value(1f, f =>
            {
                cvg_tutorial.alpha = Mathf.Lerp(1f, 0f, f);
            });
        }
    }
}