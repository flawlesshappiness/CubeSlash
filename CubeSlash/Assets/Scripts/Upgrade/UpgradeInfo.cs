using System.Collections.Generic;
using UnityEngine;

public class UpgradeInfo
{
    public Upgrade upgrade;
    public List<UpgradeInfo> parents = new List<UpgradeInfo>();
    public List<UpgradeInfo> children = new List<UpgradeInfo>();
    public bool isUnlocked;

    public UpgradeInfo(Upgrade upgrade)
    {
        this.upgrade = upgrade;
    }
}