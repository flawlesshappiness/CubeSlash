using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityController : Singleton
{
    public static AbilityController Instance { get { return Instance<AbilityController>(); } }

    private AbilityDatabase DB { get; set; }
    private List<Ability> abilities = new List<Ability>();
    private Dictionary<PlayerInput.ButtonType, Ability> equipment = new Dictionary<PlayerInput.ButtonType, Ability>();

    protected override void Initialize()
    {
        base.Initialize();
        DB = Resources.Load<AbilityDatabase>("Databases/" + nameof(AbilityDatabase));
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

    #region GAIN
    public bool CanGainAbility() => GetAvailableAbilities().Count > 0;
    public List<Ability> GetAvailableAbilities()
    {
        var gained_types = abilities.Select(ability => ability.Info.type).ToList();
        return System.Enum.GetValues(typeof(Ability.Type)).Cast<Ability.Type>()
            .Where(type => IsValid(type))
            .Select(type => DB.GetAbility(type)).ToList();

        bool IsValid(Ability.Type type)
        {
            var not_gained = !gained_types.Contains(type);
            var unlocked = IsAbilityUnlocked(type);
            return not_gained && unlocked;
        }
    }

    public Ability GetAbility(Ability.Type type) => DB.GetAbility(type);
    public List<Ability> GetGainedAbilities() => abilities.ToList();
    public bool HasAbility(Ability.Type type) => abilities.Any(a => a.Info.type == type);
    public Ability GainAbility(Ability.Type type)
    {
        var prefab = DB.GetAbility(type);
        var ability = Instantiate(prefab.gameObject).GetComponent<Ability>();
        if (ability)
        {
            abilities.Add(ability);
            Player.Instance.AttachAbility(ability);
            ability.InitializeFirstTime();
        }
        return ability;
    }

    public void GainAllAbilities()
    {
        System.Enum.GetValues(typeof(Ability.Type)).Cast<Ability.Type>().ToArray()
            .Where(type => !HasAbility(type))
            .ToList().ForEach(type => GainAbility(type));
    }

    public bool IsAbilityUnlocked(Ability.Type type)
    {
        var db = Database.Load<PlayerBodySettingsDatabase>();
        var entry = db.collection.FirstOrDefault(e => e.ability_type == type);
        if (entry == null) return true;
        if (entry.shop_product == null) return true;
        return entry.shop_product.IsUnlocked();
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