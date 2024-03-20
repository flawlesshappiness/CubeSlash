using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RadialMenu : MonoBehaviour
{
    [SerializeField] private RadialMenuElement element_template;
    [SerializeField] private RadialMenuElementSelector selector;

    public bool SubmitAnimationEnabled { get; set; } = true;
    public RadialMenuElement CurrentElement { get; private set; }
    public RadialMenuElement CancelElement { get; private set; }
    public int ElementCount => selector.Elements.Count;

    public event System.Action<RadialMenuElement> OnSelect;
    public event System.Action<RadialMenuElement> OnSubmitBegin;
    public event System.Action<RadialMenuElement> OnSubmitComplete;

    private bool shown = false;
    private bool submitting = true;

    private const string ID_CR_SHOW = "animate_show";

    private void Awake()
    {
        element_template.gameObject.SetActive(false);
    }

    public void Clear()
    {
        CurrentElement = null;
        selector.Clear();
    }

    public void AddOptions(List<RadialMenuOption> options)
    {
        options.ForEach(option => CreateElementFromOption(option));
        selector.UpdateElements();

        submitting = false;
    }

    public void AddOption(RadialMenuOption option)
    {
        CreateElementFromOption(option);
        selector.UpdateElements();

        submitting = false;
    }

    private void CreateElementFromOption(RadialMenuOption option)
    {
        var element = Instantiate(element_template, element_template.transform.parent);
        element.gameObject.SetActive(true);
        element.Initialize(option);
        element.UpdateVisual();
    }

    public RadialMenuElement GetElement(int i)
    {
        var elements = selector.Elements;
        return elements[Mathf.Clamp(i, 0, elements.Count - 1)];
    }

    public void BeginSubmit()
    {
        if (!shown) return;
        if (submitting) return;
        if (CurrentElement == null) return;
        if (CurrentElement.Option.IsLocked) return;

        submitting = true;
        CurrentElement.Option.OnSubmitBegin?.Invoke();
        OnSubmitBegin?.Invoke(CurrentElement);
        CurrentElement.UpdateVisual();

        if (SubmitAnimationEnabled)
        {
            AnimateSubmit();
        }
        else
        {
            Submit();
        }
    }

    private void Submit()
    {
        CurrentElement.Option.OnSubmitComplete?.Invoke();
        OnSubmitComplete?.Invoke(CurrentElement);
    }

    public void SetCancelElement(RadialMenuElement element)
    {
        CancelElement = element;
    }

    public void Cancel()
    {
        if (CancelElement == null) return;

        SetCurrentElement(CancelElement);
        BeginSubmit();
    }

    public void SelectElement(Vector2 direction)
    {
        if (selector == null) return;
        if (submitting) return;

        var element = selector.GetElement(direction);
        if (element == CurrentElement) return;

        SetCurrentElement(element);
    }

    public void SetCurrentElement(RadialMenuElement element)
    {
        if (selector == null) return;
        if (submitting) return;

        if (CurrentElement != null)
        {
            CurrentElement.AnimateSelect(false);
        }

        CurrentElement = element;
        OnSelect?.Invoke(element);

        if (CurrentElement != null)
        {
            CurrentElement.Option.IsNew = false;
            CurrentElement.Option.OnSelect?.Invoke();
            CurrentElement.UpdateVisual(true);
            CurrentElement.AnimateSelect(true);
        }
    }

    public CustomCoroutine AnimateShowElements(bool show, float delay = 0)
    {
        return this.StartCoroutineWithID(Cr(), ID_CR_SHOW);
        IEnumerator Cr()
        {
            shown = true;
            foreach (var element in selector.Elements)
            {
                element.AnimateShow(show);

                if (delay > 0)
                {
                    yield return new WaitForSecondsRealtime(delay);
                }
            }

            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    public CustomCoroutine AnimateSubmit()
    {
        return this.StartCoroutineWithID(Cr(), ID_CR_SHOW);
        IEnumerator Cr()
        {
            CurrentElement.AnimateSubmit();
            selector.Elements
                .Where(e => e != CurrentElement)
                .ToList().ForEach(e => e.AnimateShow(false));

            yield return new WaitForSecondsRealtime(0.5f);

            Submit();
        }
    }
}