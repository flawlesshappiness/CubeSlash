using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeLevelDatabase", menuName = "Game/UpgradeLevelDatabase", order = 1)]
public class UpgradeLevelDatabase : ScriptableObject
{
    public List<Upgrade> levels = new List<Upgrade>();
}