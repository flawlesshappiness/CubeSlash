using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AbilityModifier
{
    [HideInInspector] public string name;
    public Ability.Type type;
    public Upgrade upgrade;
}