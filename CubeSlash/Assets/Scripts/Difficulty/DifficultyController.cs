using UnityEngine;

public class DifficultyController : Singleton
{
    public static DifficultyController Instance { get { return Instance<DifficultyController>(); } }

    public DifficultyInfo Difficulty { get; private set; }
    public float DifficultyValue { get { return Difficulty.difficulty_value; } }

    public void SetDifficulty(DifficultyInfo info)
    {
        Difficulty = info;
    }
}