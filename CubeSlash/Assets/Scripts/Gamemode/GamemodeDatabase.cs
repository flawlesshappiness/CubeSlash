using UnityEngine;

[CreateAssetMenu(fileName = nameof(GamemodeDatabase), menuName = "Game/" + nameof(GamemodeDatabase), order = 1)]
public class GamemodeDatabase : Database<GamemodeSettings>
{
    public GamemodeSettings Endless;
}