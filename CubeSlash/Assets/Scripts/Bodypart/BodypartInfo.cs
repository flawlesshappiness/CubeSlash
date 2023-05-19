using UnityEngine;

[CreateAssetMenu(fileName = nameof(BodypartInfo), menuName = "Game/" + nameof(BodypartInfo), order = 1)]
public class BodypartInfo : ScriptableObject
{
    public BodypartType type;
    public Sprite preview;
    public Bodypart prefab;
    public bool is_ability_part;
}