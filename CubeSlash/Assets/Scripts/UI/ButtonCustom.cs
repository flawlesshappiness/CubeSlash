using UnityEngine;
using UnityEngine.UI;

public class ButtonCustom : ButtonExtended
{
    protected override void OnValidate()
    {
        base.Start();
        var palette = ColorPalette.Main;

        var cs = new ColorBlock();
        cs.normalColor = palette.Get(ColorPalette.Type.PRIMARY);
        cs.highlightedColor = palette.Get(ColorPalette.Type.HIGHLIGHT);
        cs.selectedColor = palette.Get(ColorPalette.Type.HIGHLIGHT);
        cs.pressedColor = palette.Get(ColorPalette.Type.HIGHLIGHT);
        cs.disabledColor = palette.Get(ColorPalette.Type.BACKGROUND);
        cs.colorMultiplier = 1;
        cs.fadeDuration = 0.1f;
        colors = cs;

        Canvas.ForceUpdateCanvases();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        OnHoverChanged += SelectIfHovered;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        OnHoverChanged -= SelectIfHovered;
    }

    private void SelectIfHovered(bool hover)
    {
        if (hover)
        {
            Select();
        }
    }
}