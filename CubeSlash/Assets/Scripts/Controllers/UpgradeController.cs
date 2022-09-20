using Flawliz.Console;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeController : Singleton
{
    public static UpgradeController Instance { get { return Instance<UpgradeController>(); } }

    public UpgradeDatabase Database { get; set; }
    private Dictionary<string, UpgradeInfo> upgrades = new Dictionary<string, UpgradeInfo>();

    protected override void Initialize()
    {
        base.Initialize();
        Database = Resources.Load<UpgradeDatabase>("Databases/" + nameof(UpgradeDatabase));
        InitializeUpgrades();

        ConsoleController.Instance.RegisterCommand("ShowUnlockedUpgrades", PrintUnlockedUpgrades);
        ConsoleController.Instance.RegisterCommand("ShowUnlockableUpgrades", PrintUnlockableUpgrades);
        ConsoleController.Instance.RegisterCommand("UnlockAllUpgrades", UnlockAllUpgrades);
        ConsoleController.Instance.RegisterCommand("UnlockUpgrade", UnlockUpgrade);
    }

    private void InitializeUpgrades()
    {
        upgrades.Clear();
        foreach (var tree in Database.trees)
        {
            TraverseRec(tree.GetRootNode(), null, tree);
        }

        void TraverseRec(UpgradeNodeData node, UpgradeInfo parent, UpgradeNodeTree tree)
        {
            UpgradeInfo info = null;

            if (upgrades.ContainsKey(node.id_name))
            {
                info = upgrades[node.id_name];
            }
            else
            {
                var upgrade = Database.upgrades.FirstOrDefault(u => u.id == node.id_name);
                info = new UpgradeInfo(upgrade);
                upgrades.Add(node.id_name, info);
            }

            if (parent != null)
            {
                parent.children.Add(info);
                info.parents.Add(parent);
            }

            foreach(var child in node.children)
            {
                TraverseRec(tree.GetNode(child), info, tree);
            }
        }
    }

    public void UnlockUpgrade(string id) => GetUpgrade(id).isUnlocked = true;
    public bool IsUpgradeUnlocked(string id) => GetUpgrade(id).isUnlocked;
    public UpgradeInfo GetUpgrade(string id) => upgrades[id];
    public UpgradeNodeTree GetUpgradeTree(string id) => Database.trees.FirstOrDefault(tree => tree.Contains(id));

    public List<UpgradeInfo> GetUnlockableUpgrades()
    {
        return upgrades.Values
            .Where(info => info.isUnlocked)
            .SelectMany(info => info.children)
            .Where(info => !IsUpgradeUnlocked(info.upgrade.id))
            .ToList();
    }

    public List<UpgradeInfo> GetUnlockedUpgrades() => upgrades.Values.Where(info => info.isUnlocked).ToList();
    public bool CanUnlockUpgrade() => GetUnlockableUpgrades().Count > 0;
    public void ClearUpgrades() => upgrades.Values.ToList().ForEach(info => info.isUnlocked = false);

    private void PrintUnlockedUpgrades()
    {
        PrintUpgrades(GetUnlockedUpgrades().Select(info => info.upgrade).ToList());
    }

    private void PrintUnlockableUpgrades()
    {
        PrintUpgrades(GetUnlockableUpgrades().Select(info => info.upgrade).ToList());
    }

    private void PrintUpgrades(List<Upgrade> upgrades)
    {
        var text = "";
        var first = true;
        foreach (var upgrade in upgrades)
        {
            if (!first) text += "\n";
            first = false;
            text += $"{upgrade.id}: \"{upgrade.name}\"";
        }

        ConsoleController.Instance.LogOutput(text);
    }

    private void UnlockAllUpgrades()
    {
        upgrades.Values.ToList().ForEach(info => info.isUnlocked = true);
    }

    private void UnlockUpgrade(string[] args)
    {
        if(args.Length > 1)
        {
            var id = args[1];
            if (upgrades.ContainsKey(id))
            {
                var info = upgrades[id];
                if (info.isUnlocked)
                {
                    ConsoleController.Instance.LogOutput("Already unlocked");
                }
                else
                {
                    UnlockUpgrade(id);
                    ConsoleController.Instance.LogOutput("Success!");
                }
            }
            else
            {
                ConsoleController.Instance.LogOutput("Upgrade does not exist");
            }
        }
        else
        {
            ConsoleController.Instance.LogOutput("ERROR: No argument");
        }
    }
}