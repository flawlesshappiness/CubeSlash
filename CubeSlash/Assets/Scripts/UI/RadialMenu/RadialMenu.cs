using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RadialMenu : MonoBehaviour
{
    [SerializeField] private RadialMenuElement element_template;
    [SerializeField] private RadialMenuElementSelector selector;

    public RadialMenuElement CurrentElement { get; private set; }

    public event System.Action<RadialMenuElement> OnSelect;
    public event System.Action<RadialMenuElement> OnSubmit;
    public event System.Action<RadialMenuElement> OnSubmitEnd;

    private bool submitting;

    private void Awake()
    {
        element_template.gameObject.SetActive(false);
    }

    public void AddOptions(List<RadialMenuOption> options)
    {
        options.ForEach(option => CreateElementFromOption(option));
        selector.UpdateElements();
    }

    public void AddOption(RadialMenuOption option)
    {
        CreateElementFromOption(option);
        selector.UpdateElements();
    }

    private void CreateElementFromOption(RadialMenuOption option)
    {
        var element = Instantiate(element_template, element_template.transform.parent);
        element.gameObject.SetActive(true);
        element.Initialize(option);
    }

    public void Submit()
    {
        if (submitting) return;
        if (CurrentElement == null) return;

        CurrentElement.Submit();
        OnSubmit?.Invoke(CurrentElement);

        selector.Elements
            .Where(e => e != CurrentElement)
            .ToList().ForEach(e => e.Hide());

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            submitting = true;

            yield return new WaitForSecondsRealtime(0.5f);

            CurrentElement.Option.OnSubmit?.Invoke();
            OnSubmitEnd?.Invoke(CurrentElement);
        }
    }

    public void SelectElement(Vector2 direction)
    {
        if (selector == null) return;

        var element = selector.GetElement(direction);
        if (element == CurrentElement) return;

        SetCurrentElement(element);
    }

    public void SetCurrentElement(RadialMenuElement element)
    {
        if (submitting) return;

        if(CurrentElement != null)
        {
            CurrentElement.Deselect();
        }

        CurrentElement = element;
        OnSelect?.Invoke(element);

        if(CurrentElement != null)
        {
            CurrentElement.Select();
        }
    }
}