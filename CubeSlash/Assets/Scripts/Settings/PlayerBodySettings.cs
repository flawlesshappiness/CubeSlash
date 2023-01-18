using UnityEngine;

[CreateAssetMenu(fileName = nameof(PlayerBodySettings), menuName = "Game/" + nameof(PlayerBodySettings), order = 1)]
public class PlayerBodySettings : ScriptableObject
{
    [Header("SETTINGS")]
    public int health;
    public int armor;
    public float body_size;
    public float mass;
    public float linear_velocity; 
    public float linear_acceleration; 
    public float linear_drag; 

    [Header("BODY")]
    public PlayerBody body;

    [Header("ABILITY")]
    public Ability.Type ability_type;
}