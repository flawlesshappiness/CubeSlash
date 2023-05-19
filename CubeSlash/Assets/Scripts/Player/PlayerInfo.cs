using UnityEngine;

[CreateAssetMenu(fileName = nameof(PlayerInfo), menuName = "Game/" + nameof(PlayerInfo), order = 1)]
public class PlayerInfo : ScriptableObject
{
    private static PlayerInfo _instance;
    public static PlayerInfo Instance { get { return _instance ?? (_instance = Resources.Load<PlayerInfo>(nameof(PlayerInfo))); } }

    [Header("SETTINGS")]
    public int health;
    public int armor;
    public float body_size;
    public float mass;
    public float linear_velocity;
    public float linear_acceleration;
    public float linear_drag;
    public PlayerBodyInfo default_body;
}