using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIFloatingPanelText : MonoBehaviour
{
    [SerializeField] private TMP_Text template_text;

    private List<TMP_Text> texts = new List<TMP_Text>();

    private void Start()
    {
        template_text.gameObject.SetActive(false);
    }

    public void Clear()
    {
        foreach (var text in texts)
        {
            Destroy(text.gameObject);
        }
        texts.Clear();
    }

    public void AddText(string text)
    {
        var tmp = Instantiate(template_text, template_text.transform.parent);
        tmp.gameObject.SetActive(true);
        tmp.text = text;
        texts.Add(tmp);
    }
}