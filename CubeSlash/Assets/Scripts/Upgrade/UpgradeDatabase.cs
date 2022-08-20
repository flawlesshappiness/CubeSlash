using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/UpgradeDatabase", fileName = nameof(UpgradeDatabase), order = 0)]
public class UpgradeDatabase : ScriptableObject
{
    public List<UpgradeData> upgrades = new List<UpgradeData>();
}