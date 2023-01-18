using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(PlayerBodySettingsDatabase), menuName = "Game/" + nameof(PlayerBodySettingsDatabase), order = 1)]
public class PlayerBodySettingsDatabase : ScriptableObject
{
    public List<PlayerBodySettings> settings = new List<PlayerBodySettings>();
}