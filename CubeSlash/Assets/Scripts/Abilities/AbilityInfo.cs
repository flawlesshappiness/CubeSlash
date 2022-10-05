using UnityEngine;

[CreateAssetMenu(fileName = "AbilityInfo", menuName = "Game/AbilityInfo", order = 1)]
public class AbilityInfo : ScriptableObject
{
    public Ability.Type type;
    public string name_ability;
    [TextArea] public string desc_ability;
    public Sprite sprite_icon;
}