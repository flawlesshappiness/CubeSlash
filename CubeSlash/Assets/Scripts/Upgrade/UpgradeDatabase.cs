using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/" + nameof(UpgradeDatabase), fileName = nameof(UpgradeDatabase), order = 0)]
public class UpgradeDatabase : Database<Upgrade>
{
}