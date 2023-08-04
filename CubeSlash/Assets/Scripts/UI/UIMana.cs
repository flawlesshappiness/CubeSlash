using Flawliz.Lerp;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIMana : MonoBehaviourExtended
{
    [SerializeField] private Image img_fill;
    [SerializeField] private Image img_bar;
    [SerializeField] private ColorPaletteValue color_normal;
    [SerializeField] private ColorPaletteValue color_wrong;

    private Lerp _lerp_fill;
    private Coroutine cr_wrong;

    private void Start()
    {
        Player.Instance.heal.OnPercentChanged += OnPercentChanged;
        Player.Instance.heal.OnHeal += OnHeal;
        Player.Instance.heal.OnHealFailed += OnHealFailed;
        SetFill(Player.Instance.heal.ValuePercent);
        SetColor(color_normal.GetColor());
    }

    public void AnimateValue(float value)
    {
        var start = img_fill.fillAmount;
        var diff = Mathf.Abs(start - value);
        var duration = Calculator.DST_Time(diff, 0.7f);
        _lerp_fill = Lerp.Value("fillamount_" + GetInstanceID(), duration, f => SetFill(Mathf.Lerp(start, value, f)));
    }

    public void AnimateWrong()
    {
        TryStopCoroutine(cr_wrong);
        cr_wrong = StartCoroutine(Cr());

        IEnumerator Cr()
        {
            for (int i = 0; i < 5; i++)
            {
                SetColor(color_wrong.GetColor());
                yield return new WaitForSeconds(0.1f);
                SetColor(color_normal.GetColor());
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private void SetColor(Color color)
    {
        img_fill.color = color;
        img_bar.color = color;
    }

    public void SetFill(float fill)
    {
        img_fill.fillAmount = Mathf.Clamp01(fill);
    }

    private void OnPercentChanged(float percent)
    {
        AnimateValue(percent);
    }

    private void OnHeal()
    {
        if (_lerp_fill != null) _lerp_fill.Kill();
        SetFill(0);
    }

    private void OnHealFailed()
    {
        if (!Player.Instance.heal.IsManaFull())
        {
            AnimateWrong();
        }
    }
}