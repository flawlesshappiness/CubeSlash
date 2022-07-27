using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImagePalette : MonoBehaviour
{
    public ColorPalette.Type type;
    public Image image;

    private void OnValidate()
    {
        if(image == null)
        {
            image = GetComponent<Image>();
        }

        var palette = ColorPalette.Main;
        if(palette != null)
        {
            image.color = ColorPalette.Main.Get(type);
        }
    }
}