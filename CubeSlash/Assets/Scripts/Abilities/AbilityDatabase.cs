using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityDatabase", menuName = "Game/AbilityDatabase", order = 1)]
public class AbilityDatabase : Database<Ability>
{
    public Ability GetAbility(Ability.Type type) => collection.FirstOrDefault(a => a.Info.type == type);
}