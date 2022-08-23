using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSettings", menuName = "Game/CharacterSettings", order = 1)]
public class CharacterSettings : ScriptableObject
{
    [Header("CHARACTER")]
    public Character character;
    public float linear_acceleration;
    public float linear_velocity;
    public float angular_acceleration;
    public float angular_velocity;
    public float size = 1;
    public float mass = 1;
}