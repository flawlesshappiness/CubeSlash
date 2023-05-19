using UnityEngine;

public class GameStateController : Singleton
{
    public static GameStateController Instance { get { return Instance<GameStateController>(); } }

    public GameStateType GameState { get; private set; }

    public System.Action<GameStateType> onGameStateChanged;

    protected override void Initialize()
    {
        base.Initialize();
        SetGameState(GameStateType.MENU);
    }

    public void SetGameState(GameStateType type)
    {
        GameState = type;
        onGameStateChanged?.Invoke(type);
    }
}

public enum GameStateType { PLAYING, MENU }