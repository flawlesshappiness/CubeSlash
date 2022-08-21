using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityDatabase", menuName = "Game/AbilityDatabase", order = 1)]
public class AbilityDatabase : ScriptableObject
{
    public List<Ability> abilities = new List<Ability>();

    public Ability GetAbility(Ability.Type type) => abilities.FirstOrDefault(a => a.type == type);
}