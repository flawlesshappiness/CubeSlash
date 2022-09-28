using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AbilityModifierEffects
{
    public Ability.Type type;
    public List<Upgrade.Effect> effects = new List<Upgrade.Effect>();
}