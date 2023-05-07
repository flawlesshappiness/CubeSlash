using UnityEngine;

[CreateAssetMenu(fileName = nameof(IconInfo), menuName = "Game/" + nameof(IconInfo), order = 1)]
public class IconInfo : ScriptableObject
{
    public IconType type;
    public Sprite sprite;
}