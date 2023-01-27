using System;
using System.Collections.Generic;
using UnityEngine;

public class UISlider : MonoBehaviour
{
    [SerializeField] private UISliderNotch template_notch;
    [SerializeField] public ButtonExtended btn;
    public int max_notches;
    public FMODEventReference sfx_change_value;


    public System.Action onValueChanged;

    private bool initialized;
    private bool is_select;
    private int selected_index;
    private UISliderNotch selected_notch;
    private List<UISliderNotch> notches = new List<UISliderNotch>();

    private void Start()
    {
        Initialize();
        btn.OnSelectedChanged += OnSelect;

        PlayerInput.Controls.UI.Navigate.started += AdjustVolume;
    }

    private void AdjustVolume(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!is_select) return;
        var v = obj.ReadValue<Vector2>();
        if(v.x > 0.1f)
        {
            IncrementValue();
        }
        else if(v.x < -0.1f)
        {
            DecrementValue();
        }
    }

    private void OnDisable()
    {
        PlayerInput.Controls.UI.Navigate.started -= AdjustVolume;
    }

    public void Initialize()
    {
        if (initialized) return;
        CreateNotches();
        initialized = true;
    }

    private void OnSelect(bool select)
    {
        is_select = select;
    }

    public void SetValue(int value)
    {
        Initialize();
        value = Mathf.Clamp(value, 0, max_notches - 1);
        var notch = notches[value];
        SetSelectedNotch(notch);
    }

    public void SetValue(float t)
    {
        var i = Mathf.RoundToInt((max_notches - 1) * t);
        SetValue(i);
    }

    public void IncrementValue() => SetValue(selected_index + 1);
    public void DecrementValue() => SetValue(selected_index - 1);

    public float GetPercentage()
    {
        var i = selected_index;
        var p = (float)i / (max_notches - 1);
        return p;
    }

    private void ClearNotches()
    {
        foreach(var notch in notches)
        {
            if (notch == null) continue;
            Destroy(notch.gameObject);
        }

        notches.Clear();
    }

    private void CreateNotches()
    {
        // Clear
        ClearNotches();

        // Create
        if (template_notch == null) return;
        template_notch.gameObject.SetActive(false);
        for (int i = 0; i < max_notches; i++)
        {
            var notch = Instantiate(template_notch, template_notch.transform.parent);
            notch.Index = i;
            notch.gameObject.SetActive(true);
            notch.SetSelected(false);
            notches.Add(notch);

            notch.onClicked += () => ClickNotch(notch);
        }

        void ClickNotch(UISliderNotch notch)
        {
            SetSelectedNotch(notch);
        }
    }

    private void SetSelectedNotch(UISliderNotch notch)
    {
        if(selected_notch != null)
        {
            selected_notch.SetSelected(false);
        }

        sfx_change_value.Play();

        selected_notch = notch;
        selected_notch.SetSelected(true);
        selected_index = selected_notch.Index;
        onValueChanged?.Invoke();
    }
}