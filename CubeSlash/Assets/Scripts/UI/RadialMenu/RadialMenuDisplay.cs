using Flawliz.Lerp;
using TMPro;
using UnityEngine;

public class RadialMenuDisplay : MonoBehaviour
{
    [SerializeField] private RadialMenu menu;
    [SerializeField] private RectTransform pivot;
    [SerializeField] private TMP_Text tmp_title, tmp_desc;

    private void Start()
    {
        menu ??= GetComponentInParent<RadialMenu>();
        menu.OnSelect += OnElementSelected;
        menu.OnSubmitBegin += OnSubmit;

        pivot.localScale = Vector3.zero;
    }

    private void OnValidate()
    {
        menu ??= GetComponentInParent<RadialMenu>();
    }

    private void OnElementSelected(RadialMenuElement element)
    {
        var show = element != null && (!string.IsNullOrEmpty(element.Option.Title) || !string.IsNullOrEmpty(element.Option.Description));

        if(!show)
        {
            ClearSelection();
            return;
        }

        var option = element.Option;
        
        if(tmp_title != null)
        {
            tmp_title.text = option.Title;
            tmp_title.gameObject.SetActive(!string.IsNullOrEmpty(option.Title));
        }

        if (tmp_desc != null)
        {
            tmp_desc.text = option.Description;
            tmp_desc.gameObject.SetActive(!string.IsNullOrEmpty(option.Description));
        }

        Lerp.LocalScale(pivot, 0.25f, Vector3.one).Curve(EasingCurves.EaseOutQuad);
    }

    private void ClearSelection()
    {
        Lerp.LocalScale(pivot, 0.25f, Vector3.zero).Curve(EasingCurves.EaseInQuad);
    }

    private void OnSubmit(RadialMenuElement element)
    {
        ClearSelection();
    }
}