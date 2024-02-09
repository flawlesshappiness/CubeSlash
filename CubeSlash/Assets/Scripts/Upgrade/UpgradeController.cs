using System.Collections.Generic;
using System.Linq;

public class UpgradeController : Singleton
{
    public static UpgradeController Instance { get { return Instance<UpgradeController>(); } }

    private UpgradeDatabase database;
    private Dictionary<UpgradeID, UpgradeInfo> upgrades = new Dictionary<UpgradeID, UpgradeInfo>();

    public System.Action<UpgradeInfo> onUpgradeUnlocked;

    protected override void Initialize()
    {
        LogController.LogMethod();

        base.Initialize();
        database = Database.Load<UpgradeDatabase>();
        InitializeUpgrades();
    }

    private void InitializeUpgrades()
    {
        LogController.LogMethod();

        upgrades.Clear();
        foreach (var upgrade in database.collection)
        {
            if (upgrades.ContainsKey(upgrade.id))
            {
                var info = upgrades[upgrade.id];
                if (info.upgrade == null)
                {
                    info.upgrade = upgrade;
                }
            }
            else
            {
                var info = new UpgradeInfo(upgrade);
                upgrades.Add(upgrade.id, info);
            }
        }
    }

    public void UnlockUpgrade(UpgradeID id)
    {
        LogController.LogMethod($"{id}");

        var info = GetUpgradeInfo(id);
        info.is_unlocked = true;
        AddGameAttributeModifiers(info);
        onUpgradeUnlocked?.Invoke(info);
    }

    public void LockUpgrade(UpgradeID id)
    {
        LogController.LogMethod($"{id}");

        var info = GetUpgradeInfo(id);
        info.is_unlocked = false;
        RemoveGameAttributeModifiers(info);
    }

    public bool IsUpgradeUnlocked(UpgradeID id) => GetUpgradeInfo(id).is_unlocked;
    public UpgradeInfo GetUpgradeInfo(UpgradeID id)
    {
        if (upgrades.TryGetValue(id, out var info))
        {
            return info;
        }
        LogController.LogMessage($"UpgradeController.GetUpgradeInfo({id}): No upgrade with that ID");
        return null;
    }
    public List<UpgradeInfo> GetUpgradeInfos() => upgrades.Values.ToList();

    public List<UpgradeInfo> GetUnlockableUpgrades()
    {
        return upgrades.Values
            .Where(info => IsValid(info))
            .ToList();

        bool IsValid(UpgradeInfo info)
        {
            var is_locked = !info.is_unlocked;
            var is_not_hidden = !info.upgrade.hidden;
            var has_required_upgrades = info.upgrade.upgrades_required.All(id => IsUpgradeUnlocked(id));
            var has_required_ability = !info.upgrade.require_ability || AbilityController.Instance.IsAbilityEquipped(info.upgrade.ability_required);
            return is_locked && is_not_hidden && has_required_upgrades && has_required_ability;
        }
    }

    public List<UpgradeInfo> GetUnlockedUpgrades() => upgrades.Values.Where(info => info.is_unlocked).ToList();
    public List<UpgradeInfo> GetUnlockedAbilityUpgrades(Ability.Type ability)
    {
        return upgrades.Values.Where(info => IsValid(info)).ToList();
        bool IsValid(UpgradeInfo info)
        {
            var is_unlocked = info.is_unlocked;
            var has_required_ability = info.upgrade.require_ability && info.upgrade.ability_required == ability;
            return is_unlocked && has_required_ability;
        }
    }

    public List<UpgradeInfo> GetUnlockedPlayerUpgrades()
    {
        return upgrades.Values.Where(info => IsValid(info)).ToList();
        bool IsValid(UpgradeInfo info)
        {
            var is_unlocked = info.is_unlocked;
            var no_required_ability = !info.upgrade.require_ability;
            return is_unlocked && no_required_ability;
        }
    }

    public List<UpgradeInfo> GetChildUpgrades(UpgradeInfo info)
    {
        var id = info.upgrade.id;
        return upgrades.Values.Where(i => i.upgrade.upgrades_required.Contains(id)).ToList();
    }

    public bool HasUnlockableUpgrades() => GetUnlockableUpgrades().Count > 0;
    public void ClearUpgrades() => upgrades.Values.ToList().ForEach(info => info.is_unlocked = false);

    public void CheatUnlockUpgrade(UpgradeInfo info)
    {
        UnlockUpgrade(info.upgrade.id);
    }

    private void AddGameAttributeModifiers(UpgradeInfo info)
    {
        var modifiers = info.upgrade.modifiers;
        foreach (var modifier in modifiers)
        {
            var attribute = GameAttributeController.Instance.GetAttribute(modifier.attribute_type);
            if (attribute == null) continue;
            attribute.AddModifier(modifier);
        }
    }

    private void RemoveGameAttributeModifiers(UpgradeInfo info)
    {
        var modifiers = info.upgrade.modifiers;
        foreach (var modifier in modifiers)
        {
            var attribute = GameAttributeController.Instance.GetAttribute(modifier.attribute_type);
            if (attribute == null) continue;
            attribute.RemoveModifier(modifier);
        }
    }
}