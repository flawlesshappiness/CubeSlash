using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Game/Player/Settings", order = 1)]
public class PlayerSettings : CharacterSettings
{
    [Header("EXPERIENCE")]
    public AnimationCurve curve_experience;
}