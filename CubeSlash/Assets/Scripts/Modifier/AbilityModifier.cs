using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AbilityModifier
{
    public Ability.Type type;
    public UpgradeID id;
    [HideInInspector] public string name;
    [TextArea]public string description;
}