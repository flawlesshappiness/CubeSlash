using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityController : Singleton
{
    public static AbilityController Instance { get { return Instance<AbilityController>(); } }

    private AbilityDatabase Database { get; set; }
    private List<Ability> abilities = new List<Ability>();
    private Dictionary<PlayerInput.ButtonType, Ability> equipment = new Dictionary<PlayerInput.ButtonType, Ability>();

    protected override void Initialize()
    {
        base.Initialize();
        Database = Resources.Load<AbilityDatabase>("Databases/" + nameof(AbilityDatabase));
        Clear();
    }

    private void InitializeEquipment()
    {
        equipment.Clear();
        equipment.Add(PlayerInput.ButtonType.NORTH, null);
        equipment.Add(PlayerInput.ButtonType.EAST, null);
        equipment.Add(PlayerInput.ButtonType.SOUTH, null);
        equipment.Add(PlayerInput.ButtonType.WEST, null);
    }

    public void Clear()
    {
        abilities.ForEach(a => Destroy(a.gameObject));
        abilities.Clear();
        InitializeEquipment();
    }

    #region UNLOCK
    public bool CanUnlockAbility() => GetUnlockableAbilities().Count > 0;
    public List<Ability> GetUnlockableAbilities()
    {
        var unlocked_types = abilities.Select(ability => ability.Info.type).ToList();
        return System.Enum.GetValues(typeof(Ability.Type)).Cast<Ability.Type>()
            .Where(type => !unlocked_types.Contains(type))
            .Select(type => Database.GetAbility(type)).ToList();
    }

    public Ability GetAbility(Ability.Type type) => Database.GetAbility(type);
    public List<Ability> GetUnlockedAbilities() => abilities.ToList();
    public bool IsAbilityUnlocked(Ability.Type type) => abilities.Any(a => a.Info.type == type);
    public Ability UnlockAbility(Ability.Type type)
    {
        var prefab = Database.GetAbility(type);
        var ability = Instantiate(prefab.gameObject).GetComponent<Ability>();
        if (ability)
        {
            abilities.Add(ability);
            Player.Instance.AttachAbility(ability);
            ability.InitializeFirstTime();
        }
        return ability;
    }

    public void UnlockAllAbilities()
    {
        System.Enum.GetValues(typeof(Ability.Type)).Cast<Ability.Type>().ToArray()
            .Where(type => !IsAbilityUnlocked(type))
            .ToList().ForEach(type => UnlockAbility(type));
    }
    #endregion
    #region EQUIP
    public void EquipAbility(Ability ability, PlayerInput.ButtonType button)
    {
        UnequipAbility(button);
        equipment[button] = ability;
        ability.Equip();
    }

    public Ability GetEquippedAbility(PlayerInput.ButtonType type) => equipment[type];
    public List<Ability> GetEquippedAbilities() => equipment.Values.Where(a => a != null).ToList();
    #endregion
    #region UNEQUIP
    public void UnequipAbility(PlayerInput.ButtonType button)
    {
        var ability = equipment[button];
        if (ability == null) return;
        ability.Unequip();
        equipment[button] = null;
    }

    public void UnequipAllAbilities()
    {
        equipment.Keys.ToList().ForEach(button =>
        {
            UnequipAbility(button);
        });
    }
    #endregion
}