using UnityEngine;

[CreateAssetMenu(fileName = nameof(GameAttributeDatabase), menuName = "Game/" + nameof(GameAttributeDatabase), order = 1)]
public class GameAttributeDatabase : Database<GameAttributeSO>
{

}