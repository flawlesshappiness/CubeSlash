using UnityEngine;

[CreateAssetMenu(fileName = nameof(ColorDatabase), menuName = "Game/" + nameof(ColorDatabase), order = 1)]
public class ColorDatabase : Database<ColorPaletteValue>
{
    public ColorPaletteValue text_normal;
    public ColorPaletteValue text_title;
    public ColorPaletteValue text_wrong;

    public static ColorDatabase Load() => Load<ColorDatabase>();
}