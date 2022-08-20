using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityController : Singleton
{
    public static AbilityController Instance { get { return Instance<AbilityController>(); } }

    public bool CanUnlockAbility() => GetUnlockableAbilities().Count > 0;
    public List<Ability> GetUnlockableAbilities()
    {
        var unlocked_types = Player.Instance.AbilitiesUnlocked.Select(ability => ability.type).ToList();
        var abilities =
            System.Enum.GetValues(typeof(Ability.Type)).Cast<Ability.Type>()
            .Where(type => !unlocked_types.Contains(type))
            .Select(type => Ability.GetPrefab(type)).ToList();
        return abilities;
    }
}