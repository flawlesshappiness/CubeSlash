using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityController : Singleton
{
    public static AbilityController Instance { get { return Instance<AbilityController>(); } }

    private AbilityDatabase DB { get; set; }
    private List<Ability> abilities = new List<Ability>();

    private Ability.Type equipped_ability;
    private List<Ability.Type> modifiers = new List<Ability.Type>();

    protected override void Initialize()
    {
        base.Initialize();
        DB = Database.Load<AbilityDatabase>();
        Clear();
    }

    public void Clear()
    {
        abilities.ForEach(a => Destroy(a.gameObject));
        abilities.Clear();
        modifiers.Clear();
    }

    #region GAIN
    public List<Ability> GetAbilities()
    {
        return DB.collection.ToList();
    }

    public bool CanGainAbility() => GetAvailableAbilities().Count > 0;
    public List<Ability> GetAvailableAbilities()
    {
        var gained_types = abilities.Select(ability => ability.Info.type).ToList();
        var ability_types = DB.collection.Select(a => a.Info.type);

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

    public Ability GetAbilityPrefab(Ability.Type type) => DB.GetAbility(type);
    public Ability GetAbility(Ability.Type type) => abilities.FirstOrDefault(a => a.Info.type == type);
    public Ability GetPrimaryAbility() => GetAbility(Save.PlayerBody.primary_ability);
    public List<Ability> GetGainedAbilities() => abilities.ToList();
    public bool HasGainedAbility(Ability.Type type) => abilities.Any(a => a.Info.type == type);
    public Ability GainAbility(Ability.Type type)
    {
        LogController.LogMethod($"{type}");

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

    private void AttachAbilityToPlayer(Ability ability)
    {
        ability.transform.position = Player.Instance.transform.position;
        ability.transform.rotation = Player.Instance.transform.rotation;
        ability.transform.localScale = Vector3.one;
        ability.transform.parent = Player.Instance.transform;
        ability.onCooldownComplete += Player.Instance.PlayCooldownCompleteFX;
    }

    public bool IsAbilityUnlocked(Ability.Type type)
    {
        return Save.Game.unlocked_abilities.Contains(type);
    }

    public void UnlockAbility(Ability.Type type)
    {
        LogController.LogMethod($"{type}");

        if (Save.Game.unlocked_abilities.Contains(type)) return;
        Save.Game.unlocked_abilities.Add(type);
        Save.Game.new_abilities.Add(type);
    }

    public AbilityInfo UnlockRandomAbility()
    {
        var abilities = GetAbilities()
            .Where(a => !IsAbilityUnlocked(a.Info.type));

        if (abilities.Count() > 0)
        {
            var ability = abilities.ToList().Random();
            UnlockAbility(ability.Info.type);
            return ability.Info;
        }

        return null;
    }
    #endregion
    #region EQUIP
    public void SetEquippedAbility(Ability.Type type)
    {
        equipped_ability = type;
    }

    public Ability GetEquippedAbility() => abilities.FirstOrDefault(a => a.Info.type == equipped_ability);
    public bool IsAbilityEquipped(Ability.Type type) => equipped_ability == type;
    #endregion
    #region MODIFIER
    public void AddModifier(Ability.Type modifier)
    {
        // Modifier
        modifiers.Add(modifier);

        // Upgrade
        var modified_ability = GetAbilityPrefab(equipped_ability);
        var upgrade = modified_ability.Info.modifiers.GetModifier(modifier);
        UpgradeController.Instance.UnlockUpgrade(upgrade.id);
    }

    public bool HasModifier(Ability.Type modifier) => modifiers.Contains(modifier);
    public List<Ability.Type> GetModifiers(Ability.Type ability) => modifiers;
    #endregion
}