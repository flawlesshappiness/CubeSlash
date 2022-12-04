using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Flawliz.Lerp;

public class GameView : View
{
    [SerializeField] private Image img_experience;

    private void Start()
    {
        Player.Instance.Experience.onValueChanged += OnExperienceChanged;
        UpdateExperience(false);
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
                .Curve(EasingCurves.EaseOutQuad);
        }
        else
        {
            img_experience.fillAmount = t;
        }
    }
}