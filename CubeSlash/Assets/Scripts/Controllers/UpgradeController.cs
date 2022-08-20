using Flawliz.Console;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeController : Singleton
{
    public static UpgradeController Instance { get { return Instance<UpgradeController>(); } }

    public UpgradeDatabase Database { get; set; }
    private Dictionary<UpgradeData.Type, Upgrade> upgrades = new Dictionary<UpgradeData.Type, Upgrade>();

    protected override void Initialize()
    {
        base.Initialize();
        Database = Resources.Load<UpgradeDatabase>("Databases/" + nameof(UpgradeDatabase));

        ConsoleController.Instance.RegisterCommand("ShowUnlockedUpgrades", PrintUnlockedUpgrades);
    }

    private bool HasUpgrade(UpgradeData.Type type) => upgrades.ContainsKey(type);
    public Upgrade GetUpgrade(UpgradeData.Type type) => HasUpgrade(type) ? upgrades[type] : AddUpgrade(type);
    private Upgrade AddUpgrade(UpgradeData.Type type)
    {
        var data = Resources.Load<UpgradeData>("Prefabs/Upgrades/" + type.ToString());
        var item = new Upgrade { data = data };
        upgrades.Add(type, item);
        return item;
    }
    public void SetUpgradeLevel(UpgradeData.Type type, int level) => GetUpgrade(type).level = level;
    public void IncrementUpgradeLevel(UpgradeData.Type type) => SetUpgradeLevel(type, GetUpgrade(type).level + 1);
    public void ClearUpgrades() => upgrades.Clear();

    public List<Upgrade> GetAbilityUpgrades(Ability.Type type)
    {
        return Database.upgrades.Where(u => u.require_ability && u.type_ability_required == type)
            .Select(data => GetUpgrade(data.type))
            .ToList();
    }

    public bool CanUnlockUpgrade() => GetUnlockableUpgrades().Count > 0;
    public List<Upgrade> GetUnlockableUpgrades()
    {
        return Database.upgrades
            .Where(u => !u.require_ability || Player.Instance.AbilitiesEquipped
                .Where(a => a != null)
                .Any(a => u.type_ability_required == a.type))
            .Select(u => GetUpgrade(u.type))
            .Where(u => !u.IsMaxLevel)
            .ToList();
    }

    public List<Upgrade> GetUnlockedUpgrades() => upgrades.Values.Where(u => u.level > 0).ToList();

    private void PrintUnlockedUpgrades()
    {
        var text = "";
        var first = true;
        foreach(var upgrade in upgrades.Values.Where(u => u.level > 0))
        {
            if (!first) text += "\n";
            first = false;
            text += $"{upgrade.data.name} ({upgrade.level})";
        }

        ConsoleController.Instance.LogOutput(text);
    }
}