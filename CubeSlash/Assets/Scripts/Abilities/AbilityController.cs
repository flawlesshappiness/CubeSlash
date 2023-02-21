using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityController : Singleton
{
    public static AbilityController Instance { get { return Instance<AbilityController>(); } }

    private AbilityDatabase DB { get; set; }
    private List<Ability> abilities = new List<Ability>();
    private Dictionary<PlayerInput.ButtonType, Ability> equipment = new Dictionary<PlayerInput.ButtonType, Ability>();
    private Dictionary<Ability.Type, List<Ability.Type>> modifiers = new Dictionary<Ability.Type, List<Ability.Type>>();

    protected override void Initialize()
    {
        base.Initialize();
        DB = Database.Load<AbilityDatabase>();
        Clear();
    }

    private void InitializeEquipment()
    {
        equipment.Clear();
        equipment.Add(PlayerInput.ButtonType.NORTH, null);
        equipment.Add(PlayerInput.ButtonType.EAST, null);
        equipment.Add(PlayerInput.ButtonType.SOUTH, null);
        equipment.Add(PlayerInput.ButtonType.WEST, null);

        // Modifiers
        var ability_types = System.Enum.GetValues(typeof(Ability.Type)).Cast<Ability.Type>();
        foreach(var type in ability_types)
        {
            modifiers.Add(type, new List<Ability.Type>());
        }
    }

    public void Clear()
    {
        abilities.ForEach(a => Destroy(a.gameObject));
        abilities.Clear();
        modifiers.Clear();
        InitializeEquipment();
    }

    #region GAIN
    public bool CanGainAbility() => GetAvailableAbilities().Count > 0;
    public List<Ability> GetAvailableAbilities()
    {
        var gained_types = abilities.Select(ability => ability.Info.type).ToList();
        var ability_types = System.Enum.GetValues(typeof(Ability.Type)).Cast<Ability.Type>();

        return ability_types
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
    public bool HasGainedAbility(Ability.Type type) => abilities.Any(a => a.Info.type == type);
    public Ability GainAbility(Ability.Type type)
    {
        var prefab = DB.GetAbility(type);
        var ability = Instantiate(prefab.gameObject).GetComponent<Ability>();
        if (ability)
        {
            abilities.Add(ability);
            AttachAbilityToPlayer(ability);
            ability.InitializeFirstTime();
        }
        return ability;
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
    }

    public Ability GetEquippedAbility(PlayerInput.ButtonType type) => equipment[type];
    public List<Ability> GetEquippedAbilities() => equipment.Values.Where(a => a != null).ToList();
    public bool IsAbilityEquipped(Ability.Type type) => equipment.Values.Any(ability => ability.Info.type == type);
    #endregion
    #region UNEQUIP
    public void UnequipAbility(PlayerInput.ButtonType button)
    {
        var ability = equipment[button];
        if (ability == null) return;
        equipment[button] = null;
        RemoveModifiers(ability.Info.type);
    }

    public void UnequipAllAbilities()
    {
        equipment.Keys.ToList().ForEach(button =>
        {
            UnequipAbility(button);
        });
    }

    private void AttachAbilityToPlayer(Ability ability)
    {
        ability.transform.position = Player.Instance.transform.position;
        ability.transform.rotation = Player.Instance.transform.rotation;
        ability.transform.localScale = Vector3.one;
        ability.transform.parent = Player.Instance.transform;
    }
    #endregion
    #region MODIFIER
    public void AddModifier(Ability.Type ability, Ability.Type modifier)
    {
        // Modifier
        modifiers[ability].Add(modifier);

        // Upgrade
        var modified_ability = GetAbility(ability);
        var upgrade = modified_ability.Info.modifiers.GetModifier(modifier);
        UpgradeController.Instance.UnlockUpgrade(upgrade.id);
    }

    public void RemoveModifiers(Ability.Type ability)
    {
        // Upgrade
        var modified_ability = GetAbility(ability);
        foreach(var modifier in modifiers[ability])
        {
            var upgrade = modified_ability.Info.modifiers.GetModifier(modifier);
            UpgradeController.Instance.LockUpgrade(upgrade.id);
        }

        // Modifier
        modifiers[ability].Clear();
    }

    public bool HasModifier(Ability.Type ability, Ability.Type modifier)
    {
        return modifiers[ability].Contains(modifier);
    }

    public Ability GetModifier(Ability.Type ability, Ability.Type modifier)
    {
        if(HasModifier(ability, modifier))
        {
            return GetAbility(modifier);
        }
        return null;
    }

    public List<Ability.Type> GetModifiers(Ability.Type ability)
    {
        return modifiers[ability];
    }
    #endregion
}