using System.Collections.Generic;
using UnityEngine;

public class UISlider : MonoBehaviour
{
    [SerializeField] private LeftRightMenuItem menu;
    [SerializeField] private UISliderNotch template_notch;

    public int max_notches;

    public System.Action onValueChanged;

    private bool initialized;
    private int selected_index;
    private UISliderNotch selected_notch;
    private List<UISliderNotch> notches = new List<UISliderNotch>();

    private void Start()
    {
        Initialize();
        menu.onMove += OnMove;
    }

    private void OnMove(int dir)
    {
        var prev_index = selected_index;
        SetValue(selected_index + dir);

        if(selected_index != prev_index)
        {
            SoundController.Instance.Play(SoundEffectType.sfx_ui_move);
        }
    }

    public void Initialize()
    {
        if (initialized) return;
        CreateNotches();
        initialized = true;
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
        }
    }

    private void SetSelectedNotch(UISliderNotch notch)
    {
        if(selected_notch != null)
        {
            selected_notch.SetSelected(false);
        }

        selected_notch = notch;
        selected_notch.SetSelected(true);
        selected_index = selected_notch.Index;
        onValueChanged?.Invoke();
    }
}