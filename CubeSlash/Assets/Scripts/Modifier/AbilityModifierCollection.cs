using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityModifierCollection", menuName = "Game/AbilityModifierCollection", order = 1)]
public class AbilityModifierCollection : ScriptableObject
{
    public List<AbilityModifier> modifiers = new List<AbilityModifier>();
    public AbilityModifier GetModifier(Ability.Type type) => modifiers.FirstOrDefault(m => m.type == type);

    private void Reset()
    {
        // Add modifiers
        var types = System.Enum.GetValues(typeof(Ability.Type)).Cast<Ability.Type>().ToList();
        foreach (var type in types)
        {
            var ms = modifiers.Where(m => m.type == type).ToList();
            if (ms.Count == 0)
            {
                modifiers.Add(new AbilityModifier { type = type });
            }
            else
            {
                for (int i = 0; i < ms.Count; i++)
                {
                    if (i > 0)
                    {
                        modifiers.Remove(ms[i]);
                    }
                }
            }
        }
    }
}