using TMPro;
using UnityEngine;

public class UIAbilityStatVariable : MonoBehaviour
{
    [SerializeField] private TMP_Text template_tmp_text;
    [SerializeField] private GameObject divider;

    public bool DividerEnabled { set { divider.SetActive(value); } }

    private void Start()
    {
        template_tmp_text.gameObject.SetActive(false);
    }

    public void SetText(string text)
    {
        CreateText(text);
    }

    public void SetFromToText(string from, string to)
    {
        CreateText(from).alignment = TextAlignmentOptions.MidlineGeoAligned;
        CreateText(">").alignment = TextAlignmentOptions.MidlineGeoAligned;
        CreateText(to).alignment = TextAlignmentOptions.MidlineGeoAligned;
    }

    private TMP_Text CreateText(string text)
    {
        var t = Instantiate(template_tmp_text, template_tmp_text.transform.parent);
        t.gameObject.SetActive(true);
        t.text = text;
        return t;
    }
}