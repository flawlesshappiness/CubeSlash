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
    private int area_refills;

    private List<Area> visited_areas = new List<Area>();
    private List<Area> available_areas = new List<Area>();

    public int AreaIndex { get { return index_area; } }
    public float TimeAreaStart { get; private set; }
    public float TimeEndlessStart { get; private set; }
    public Area CurrentArea { get { return current_area; } }
    public bool IsEndless { get { return area_refills > 0; } }

    public MultiLock NextAreaLock { get; private set; } = new MultiLock();

    private bool force_next_area;

    protected override void Initialize()
    {
        base.Initialize();
        db = AreaDatabase.Instance;
        GameController.Instance.onGameEnd += OnGameEnd;
        GameController.Instance.onMainMenu += OnMainMenu;
    }

    private void OnGameEnd()
    {
        StopAreaCoroutine();
    }

    private void OnMainMenu()
    {
        visited_areas.Clear();
        available_areas.Clear();
        force_next_area = false;
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
        area_refills = -1;
        while (true)
        {
            index_area++;
            current_area = GetNextArea();
            visited_areas.Add(current_area);
            available_areas.Remove(current_area);
            TimeAreaStart = Time.time;

            onNextArea?.Invoke(current_area);

            var time_next = Time.time + GameSettings.Instance.area_duration;
            var has_time_left = Time.time < time_next;
            var is_locked = NextAreaLock.IsLocked;
            while((is_locked || has_time_left) && !force_next_area)
            {
                yield return null;
            }
            force_next_area = false;
        }
    }

    public void ForceNextArea()
    {
        force_next_area = true;
    }

    private Area GetNextArea()
    {
        if(available_areas.Count == 0)
        {
            area_refills++;
            visited_areas.Clear();
            available_areas.AddRange(db.areas);

            // First time endless
            if (area_refills == 1)
            {
                OnEndlessBegun();
            }
        }

        return available_areas
            .Where(a => IsValid(a))
            .ToList().Random();

        bool IsValid(Area area)
        {
            var valid_index = index_area >= area.index_level_min;
            var unvisited = IsEndless || !visited_areas.Contains(area);
            return valid_index && unvisited;
        }
    }

    private void OnEndlessBegun()
    {
        TimeEndlessStart = Time.time;
    }
}