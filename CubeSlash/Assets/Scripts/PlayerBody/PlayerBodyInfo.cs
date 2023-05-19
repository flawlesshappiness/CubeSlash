using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(PlayerBodyInfo), menuName = "Game/" + nameof(PlayerBodyInfo), order = 1)]
public class PlayerBodyInfo : ScriptableObject
{
    public PlayerBodyType type;
    public PlayerBody prefab;
    public List<Sprite> skins = new List<Sprite>();
}