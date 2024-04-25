using Flawliz.Lerp;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameView : View
{
    [SerializeField] private Image img_experience;
    [SerializeField] private TMP_Text tmp_endless_timer;
    [SerializeField] private CanvasGroup cvg_tutorial;
    [SerializeField] private CanvasGroup cvg_endless_timer;

    private void Start()
    {
        UpdateExperience(false);
        cvg_tutorial.alpha = 0;
        cvg_endless_timer.alpha = RunInfo.Current.Endless ? 1 : 0;

        if (!RunInfo.Current.Endless)
        {
            ShowIntroTutorial();
        }
    }

    private void Update()
    {
        UpdateEndlessTimer();
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
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(1.0f);

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

    private void UpdateEndlessTimer()
    {
        if (!RunInfo.Current.Endless) return;

        var start = RunInfo.Current.EndlessStartTime;
        var time = Time.time - start;
        var value = Player.Instance.IsDead ? RunInfo.Current.EndlessDuration : time;
        var text = value.ToString("0.0");
        tmp_endless_timer.text = text;
    }
}