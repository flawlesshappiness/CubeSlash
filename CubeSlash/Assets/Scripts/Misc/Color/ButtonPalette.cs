using UnityEngine;

public class ButtonPalette : MonoBehaviour
{
    [SerializeField] private ButtonExtended button;

    public ColorPalette.Type Normal;
    public ColorPalette.Type Hovered;
    public ColorPalette.Type Selected;

    private bool isSelected;
    private bool isHovered;

    private void OnEnable()
    {
        button.OnSelectedChanged += OnSelectedChanged;
        button.OnHoverChanged += OnHoverChanged;
    }

    private void OnDisable()
    {
        button.OnSelectedChanged -= OnSelectedChanged;
        button.OnHoverChanged -= OnHoverChanged;
    }

    private void OnValidate()
    {
        if(button != null && button.image != null)
        {
            UpdateColor();
        }
    }

    private void OnSelectedChanged(bool selected)
    {
        isSelected = selected;
        UpdateColor();
    }

    private void OnHoverChanged(bool hovered)
    {
        isHovered = hovered;
        UpdateColor();
    }

    private void UpdateColor()
    {
        var color =
            isSelected ? ColorPalette.Main.Get(Selected) :
            isHovered ? ColorPalette.Main.Get(Hovered) :
            ColorPalette.Main.Get(Normal);
        button.image.color = color;
    }
}