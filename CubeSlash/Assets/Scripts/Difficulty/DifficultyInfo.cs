using UnityEngine;

[CreateAssetMenu(fileName = nameof(DifficultyInfo), menuName = "Game/" + nameof(DifficultyInfo), order = 1)]
public class DifficultyInfo : ScriptableObject
{
    public string difficulty_name;

    [TextArea]
    public string difficulty_description;

    [Range(0, 1)]
    public float difficulty_value;
}