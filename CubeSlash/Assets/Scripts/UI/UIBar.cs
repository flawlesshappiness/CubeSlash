using Flawliz.Lerp;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIBar : MonoBehaviour
{
    [SerializeField] private Image img_bar;
    [SerializeField] private CanvasGroup cvg;

    public CanvasGroup CanvasGroup { get { return cvg; } }

    public CustomCoroutine AnimateValue(float duration, float value, AnimationCurve curve = null)
    {
        curve = curve ?? EasingCurves.Linear;
        return this.StartCoroutineWithID(Cr(), "animate_value_"+GetInstanceID());
        IEnumerator Cr()
        {
            var start = img_bar.fillAmount;
            yield return LerpEnumerator.Value(duration, f =>
            {
                img_bar.fillAmount = Mathf.Lerp(start, value, curve.Evaluate(f));
            }).UnscaledTime();
        }
    }

    public void SetValue(float value)
    {
        img_bar.fillAmount = value;
    }
}