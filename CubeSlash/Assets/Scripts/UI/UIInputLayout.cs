using System.Collections.Generic;
using UnityEngine;

public class UIInputLayout : MonoBehaviour
{
    [SerializeField] private UIInputLayoutRow template_row;
    [SerializeField] private CanvasGroup cvg;

    public CanvasGroup CanvasGroup { get { return cvg; } }

    private List<UIInputLayoutRow> rows = new List<UIInputLayoutRow>();

    private void Start()
    {
        template_row.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        PlayerInput.OnDeviceChanged += OnDeviceChanged;
    }

    private void OnDisable()
    {
        PlayerInput.OnDeviceChanged -= OnDeviceChanged;
    }

    private void OnDeviceChanged(PlayerInput.DeviceType type)
    {
        rows.ForEach(row => row.Image.SetInputType(row.Image.type_button));
    }

    public void AddInput(PlayerInput.UIButtonType type, string text)
    {
        var row = Instantiate(template_row, template_row.transform.parent);
        row.gameObject.SetActive(true);
        rows.Add(row);

        row.Image.SetInputType(type);
        row.Text = text;
    }

    public void Clear()
    {
        foreach (var row in rows)
        {
            Destroy(row.gameObject);
        }
        rows.Clear();
    }

    public void SetupTutorial()
    {
        Clear();
        AddInput(PlayerInput.UIButtonType.NAV_ALL, "Move");
        AddInput(PlayerInput.UIButtonType.WEST, "Use ability");
        AddInput(PlayerInput.UIButtonType.EAST, "Dash");
        AddInput(PlayerInput.UIButtonType.NORTH, "Heal using energy");
    }
}