using Flawliz.Lerp;
using System.Collections;
using TMPro;
using UnityEngine;

public class UICurrencyBar : MonoBehaviour
{
    [SerializeField] private TMP_Text tmp_value;

    public bool update_on_start;

    private int current_text_value;

    public void Start()
    {
        if (update_on_start) UpdateValue();
    }

    public void UpdateValue()
    {
        SetValueText(Save.Game.currency);
    }

    public CustomCoroutine AnimateUpdateValue(float duration, AnimationCurve curve = null)
    {
        return this.StartCoroutineWithID(Cr(), "AnimateUpdateValue_" + GetInstanceID());
        IEnumerator Cr()
        {
            curve = curve ?? EasingCurves.Linear;
            var start = current_text_value;
            var end = Save.Game.currency;
            yield return LerpEnumerator.Value(duration, f =>
            {
                var t = curve.Evaluate(f);
                var v = (int)Mathf.Lerp(start, end, t);
                SetValueText(v);
            }).UnscaledTime();
            SetValueText(Save.Game.currency);
        }
    }

    public void SetValueText(int value)
    {
        value = Mathf.Min(value, 9999999);
        tmp_value.text = value.ToString();
        current_text_value = value;
    }
}