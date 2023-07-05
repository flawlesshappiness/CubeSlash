using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIFloatingPanelUpgrade : MonoBehaviour
{
    [SerializeField] private UIFloatingPanelUpgradeFromToText template_from_to_text;
    [SerializeField] private TMP_Text template_text;

    private List<GameObject> elements = new List<GameObject>();

    private void Start()
    {
        template_from_to_text.gameObject.SetActive(false);
        template_text.gameObject.SetActive(false);
    }

    public void Clear()
    {
        foreach (var element in elements.ToList())
        {
            Destroy(element.gameObject);
        }
        elements.Clear();
    }

    public void AddModifiedAttribute(GameAttribute attribute, GameAttributeModifier modifier)
    {
        var from_to = Instantiate(template_from_to_text, template_from_to_text.transform.parent);
        from_to.gameObject.SetActive(true);
        elements.Add(from_to.gameObject);

        var text = Instantiate(template_text, template_text.transform.parent);
        text.gameObject.SetActive(true);
        elements.Add(text.gameObject);

        text.text = attribute.text;
        from_to.SetTextFromAttributeModifier(attribute, modifier);
    }
}