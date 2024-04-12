using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AreaController : Singleton
{
    public static AreaController Instance { get { return Instance<AreaController>(); } }

    public event System.Action<Area> onNextArea;

    private AreaDatabase db;
    private Coroutine cr_next_area;
    private Area current_area;

    private List<Area> visited_areas = new List<Area>();
    private List<Area> available_areas = new List<Area>();

    public Area CurrentArea => current_area;
    public int CurrentAreaIndex => RunController.Instance.CurrentRun?.CurrentAreaIndex ?? 0;
    public bool IsFinalArea => CurrentAreaIndex == (RunController.Instance.CurrentRun?.Areas?.Count ?? 1);

    protected override void Initialize()
    {
        base.Initialize();
        db = Database.Load<AreaDatabase>();
        RunController.Instance.onRunStarted += RunStarted;
        GameController.Instance.onGameEnd += OnGameEnd;
        GameController.Instance.onMainMenu += OnMainMenu;
    }

    private void RunStarted()
    {
        StartAreaCoroutine();
    }

    private void OnGameEnd()
    {
        StopAreaCoroutine();
    }

    private void OnMainMenu()
    {
        visited_areas.Clear();
        available_areas = db.collection.ToList();
    }

    public void StartAreaCoroutine()
    {
        StopAreaCoroutine();
        cr_next_area = StartCoroutine(NextAreaCr());
    }

    public void StopAreaCoroutine()
    {
        if (cr_next_area != null)
        {
            StopCoroutine(cr_next_area);
            cr_next_area = null;
        }
    }

    private IEnumerator NextAreaCr()
    {
        var gamemode = GamemodeController.Instance.SelectedGameMode;
        var run = RunController.Instance.CurrentRun;
        while (CurrentAreaIndex < run.Areas.Count)
        {
            var next_area = run.Areas[CurrentAreaIndex];
            SetArea(next_area);

            var time_next_area = run.StartTime + (run.CurrentAreaIndex + 1) * gamemode.area_duration;
            while (Time.time < time_next_area)
            {
                yield return null;
            }

            run.CurrentAreaIndex++;
        }
    }

    public void SetArea(Area area)
    {
        current_area = area;
        onNextArea?.Invoke(current_area);
    }

    public void DebugSetArea(Area area)
    {
        StopAreaCoroutine();
        SetArea(area);
    }

    public List<Area> GetRandomAreas(int count)
    {
        var areas = new List<Area>();
        var available = db.collection.ToList();

        for (int i = 0; i < count; i++)
        {
            if (i == count - 1)
            {
                areas.Add(db.final_area);
            }
            else
            {
                var area = available_areas
                    .Where(a => IsValid(i, a))
                    .ToList().Random();

                available.Remove(area);

                areas.Add(area);
            }
        }

        return areas;

        bool IsValid(int i, Area area)
        {
            var valid_index = i >= area.index_level_min;
            var unvisited = !visited_areas.Contains(area);
            return valid_index && unvisited;
        }
    }
}