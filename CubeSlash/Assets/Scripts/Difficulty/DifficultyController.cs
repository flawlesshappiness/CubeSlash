using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DifficultyController : Singleton
{
    public static DifficultyController Instance { get { return Instance<DifficultyController>(); } }

    public DifficultyInfo Difficulty { get; private set; }
    public float DifficultyValue { get { return Difficulty.difficulty_value; } }
    public int DifficultyIndex { get; private set; }
    public List<DifficultyInfo> DifficultyInfos { get { return _database.collection.ToList(); } }

    private DifficultyDatabase _database;

    protected override void Initialize()
    {
        base.Initialize();
        _database = Database.Load<DifficultyDatabase>();
        SetDifficulty(Save.Game.idx_gamesetup_difficulty);
    }

    public void SetDifficulty(int i)
    {
        Difficulty = _database.collection[i];
        DifficultyIndex = i;
        Save.Game.idx_gamesetup_difficulty = i;
    }

    public void SetDifficulty(DifficultyInfo info)
    {
        var i = _database.collection.IndexOf(info);
        SetDifficulty(i);
    }
}