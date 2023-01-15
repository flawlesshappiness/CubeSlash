using UnityEngine;
using UnityEngine.UI;
using Flawliz.Lerp;
using System.Collections;

public class MenuButton : MonoBehaviour
{
    [SerializeField] private RectTransform rt_selected;
    [SerializeField] private CanvasGroup cvg_selected;
    [SerializeField] private ButtonExtended button;

    private void OnEnable()
    {
        button.OnSelectedChanged += OnSelectionChanged;
        cvg_selected.alpha = 0;
        rt_selected.sizeDelta = Vector2.right * 100f;
    }

    private void OnSelectionChanged(bool selected)
    {
        var scale_show = 0f;
        var scale_hide = 100f;
        var scale_start = rt_selected.sizeDelta;
        var scale_end = Vector2.right * (selected ? scale_show : scale_hide);
        var alpha_start = cvg_selected.alpha;
        var alpha_end = selected ? 1 : 0;

        this.StartCoroutineWithID(Cr(), "scale_select_" + GetInstanceID());
        IEnumerator Cr()
        {
            var lerp = LerpEnumerator.Value(0.15f, f =>
            {
                cvg_selected.alpha = Mathf.Lerp(alpha_start, alpha_end, f);
                rt_selected.sizeDelta = Vector2.Lerp(scale_start, scale_end, f);
            });
            lerp.UnscaledTime = true;
            yield return lerp;
        }
    }
}