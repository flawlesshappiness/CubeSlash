using System.Collections.Generic;
using UnityEngine;

public class UpgradeController : Singleton
{ 
    public static UpgradeController Instance { get { return Instance<UpgradeController>(); } }

    private Dictionary<UpgradeData.Type, Upgrade> upgrades = new Dictionary<UpgradeData.Type, Upgrade>();

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
}