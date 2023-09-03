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
    private int index_area;
    private int max_area_index;

    private List<Area> visited_areas = new List<Area>();
    private List<Area> available_areas = new List<Area>();

    public bool IsFinalArea { get; set; }
    public int AreaIndex { get { return index_area; } }
    public float TimeAreaStart { get; private set; }
    public Area CurrentArea { get { return current_area; } }

    private float time_next_area;

    protected override void Initialize()
    {
        base.Initialize();
        db = Database.Load<AreaDatabase>();
        GameController.Instance.onGameStart += OnGameStart;
        GameController.Instance.onGameEnd += OnGameEnd;
        GameController.Instance.onMainMenu += OnMainMenu;
    }

    private void OnGameStart()
    {
        var diff = DifficultyController.Instance.DifficultyValue;
        max_area_index = (int)GameSettings.Instance.area_count_difficulty.Evaluate(diff);
    }

    private void OnGameEnd()
    {
        StopAreaCoroutine();
    }

    private void OnMainMenu()
    {
        IsFinalArea = false;
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
        index_area = -1;
        while (!IsFinalArea)
        {
            index_area++;

            var next_area = GetNextArea();
            SetArea(next_area);

            visited_areas.Add(current_area);
            available_areas.Remove(current_area);

            while (Time.time < time_next_area)
            {
                yield return null;
            }
        }
    }

    public void SetArea(Area area)
    {
        current_area = area;
        TimeAreaStart = Time.time;
        time_next_area = Time.time + GameSettings.Instance.area_duration;
        onNextArea?.Invoke(current_area);
    }

    public void ForceNextArea()
    {
        time_next_area = Time.time;
    }

    public void ForceFinalArea()
    {
        available_areas.Clear();
        ForceNextArea();
    }

    private Area GetNextArea()
    {
        if (available_areas.Count == 0 || index_area >= max_area_index)
        {
            IsFinalArea = true;
            return db.final_area;
        }

        return available_areas
            .Where(a => IsValid(a))
            .ToList().Random();

        bool IsValid(Area area)
        {
            var valid_index = index_area >= area.index_level_min;
            var unvisited = !visited_areas.Contains(area);
            return valid_index && unvisited;
        }
    }
}