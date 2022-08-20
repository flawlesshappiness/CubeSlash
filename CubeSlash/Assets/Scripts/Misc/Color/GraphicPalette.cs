using UnityEngine;
using UnityEngine.UI;

public class GraphicPalette : MonoBehaviour
{
    public ColorPalette.Type type;
    public Graphic graphic;

    private void OnValidate()
    {
        if(graphic == null)
        {
            graphic = GetComponent<Graphic>();
        }

        var palette = ColorPalette.Main;
        if(palette != null)
        {
            graphic.color = ColorPalette.Main.Get(type);
        }
    }
}