using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Player/Settings", order = 1)]
public class PlayerSettings : ScriptableObject
{
    [Header("EXPERIENCE")]
    public int experience_level_max = 25;
    public int experience_min;
    public int experience_max;
    public AnimationCurve curve_experience;
}