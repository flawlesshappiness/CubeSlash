using UnityEngine;

public class DifficultyController : Singleton
{
    public static DifficultyController Instance { get { return Instance<DifficultyController>(); } }

    public DifficultyInfo Difficulty { get; private set; }
    public float DifficultyValue { get { return Difficulty.difficulty_value; } }
    public int DifficultyIndex { get; private set; }

    public void SetDifficulty(int i)
    {
        var db = Database.Load<DifficultyDatabase>();
        Difficulty = db.collection[i];
        DifficultyIndex = i;
    }
}