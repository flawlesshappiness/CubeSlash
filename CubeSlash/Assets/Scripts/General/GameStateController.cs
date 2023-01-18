using UnityEngine;

public class GameStateController : Singleton
{
    public static GameStateController Instance { get { return Instance<GameStateController>(); } }

    public GameStateType GameState { get; private set; }

    public System.Action<GameStateType> onGameStateChanged;

    public void SetGameState(GameStateType type)
    {
        GameState = type;
        onGameStateChanged?.Invoke(type);
    }
}

public enum GameStateType { PLAYING, MENU }