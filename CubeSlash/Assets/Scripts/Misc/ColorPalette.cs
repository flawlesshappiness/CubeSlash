using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "Game/ColorPalette", order = 1)]
public class ColorPalette : ScriptableObject
{
    private static ColorPalette _main;
    public static ColorPalette Main { get { return _main ?? LoadMain(); } }

    private static ColorPalette LoadMain()
    {
        _main = Resources.Load<ColorPalette>("Color/Main");
        return _main;
    }

    public Color normal = Color.white;
    public Color hover = Color.white;
    public Color selected = Color.white;
    public Color disabled = Color.white;
}