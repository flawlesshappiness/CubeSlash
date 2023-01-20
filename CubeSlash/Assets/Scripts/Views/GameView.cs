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

    private bool tutorial_is_active;

    private void Start()
    {
        UpdateExperience(false);
        OnNextLevel();
    }

    private void OnEnable()
    {
        Player.Instance.Experience.onValueChanged += OnExperienceChanged;
        GameController.Instance.OnNextLevel += OnNextLevel;
    }

    private void OnDisable()
    {
        Player.Instance.Experience.onValueChanged -= OnExperienceChanged;
        GameController.Instance.OnNextLevel -= OnNextLevel;
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

    private void OnNextLevel()
    {
        if (Level.Current.show_tutorial)
        {
            if (!tutorial_is_active)
            {
                ShowTutorial();
            }
        }
        else
        {
            if (tutorial_is_active)
            {
                HideTutorial();
            }
        }
    }

    public void ShowTutorial()
    {
        tutorial_is_active = true;
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
        }
    }
    
    public void HideTutorial()
    {
        tutorial_is_active = false;
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return LerpEnumerator.Value(1f, f =>
            {
                cvg_tutorial.alpha = Mathf.Lerp(1f, 0f, f);
            });
        }
    }
}