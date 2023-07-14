using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Game/Upgrade", order = 1)]
public class Upgrade : ScriptableObject
{
    public UpgradeID id;
    public string name;
    public Sprite icon;
    public bool hidden;
    public bool require_ability;
    public Ability.Type ability_required;
    public List<UpgradeID> upgrades_required;
    public List<GameAttributeModifier> modifiers = new List<GameAttributeModifier>();
}