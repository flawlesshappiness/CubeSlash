using System.Collections.Generic;

public class RunInfo
{
    public static RunInfo Current => RunController.Instance.CurrentRun;

    public GamemodeSettings Gamemode { get; set; } // Gamemode for this run
    public List<Area> Areas { get; set; } = new List<Area>(); // List of areas that will appear in this run
    public Dictionary<EnemyType, int> EnemiesKilled { get; set; } = new Dictionary<EnemyType, int>(); // Dictionary of enemies killed during this run 
    public int CurrentAreaIndex { get; set; }
    public float StartTime { get; set; }
    public bool Won { get; set; }
    public bool Endless { get; set; }
    public float EndlessStartTime { get; set; }
    public float EndlessEndTime { get; set; }

    public float EndlessDuration => EndlessEndTime - EndlessStartTime;
}