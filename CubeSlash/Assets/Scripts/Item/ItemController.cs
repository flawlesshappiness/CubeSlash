using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemController : Singleton
{
    public static ItemController Instance { get { return Singleton.Instance<ItemController>(); } }

    public System.Action<ExperienceItem> OnExperienceSpawned { get; set; }

    private List<ExperienceItem> experience_active = new List<ExperienceItem>();
    private List<ExperienceItem> experience_inactive = new List<ExperienceItem>();

    private ExperienceItem prefab_experience;

    private const int COUNT_POOL_EXTEND = 20;

    private void Start()
    {
        prefab_experience = Resources.Load<ExperienceItem>("Prefabs/Entities/Experience");
    }

    private void Update()
    {
        if (!GameController.Instance.IsGameStarted) return;

        SpawnExperienceUpdate();
    }

    private float time_spawn;
    private void SpawnExperienceUpdate()
    {
        if (Time.time < time_spawn) return;
        if (experience_active.Count >= Level.Current.count_experience_active) return;
        time_spawn = Time.time + Level.Current.frequency_spawn_experience;
        var e = SpawnExperience(CameraController.Instance.GetPositionOutsideCamera());
        e.Initialize();
        e.SetPlant();
    }

    public ExperienceItem SpawnExperience(Vector3 position)
    {
        if(experience_inactive.Count == 0)
        {
            ExtendPool(COUNT_POOL_EXTEND);
        }

        var e = experience_inactive.Pop();
        experience_active.Add(e);
        e.gameObject.SetActive(true);
        e.transform.position = position;

        OnExperienceSpawned?.Invoke(e);

        return e;
    }

    private void ExtendPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var e = CreateExperienceForPool();
        }
    }

    private ExperienceItem CreateExperienceForPool()
    {
        var e = Instantiate(prefab_experience.gameObject, GameController.Instance.world).GetComponent<ExperienceItem>();
        experience_inactive.Add(e);
        e.gameObject.SetActive(false);
        return e;
    }

    public void ClearAllExperience()
    {
        foreach(var e in experience_active)
        {
            e.Despawn();
        }
    }

    public void OnExperienceDespawned(ExperienceItem e)
    {
        e.gameObject.SetActive(false);
        experience_active.Remove(e);
        experience_inactive.Add(e);
    }

    public void DespawnAllActiveItems()
    {
        foreach(var item in experience_active.ToList())
        {
            item.Despawn();
        }
    }

    public List<ExperienceItem> GetActiveExperiences() => experience_active.ToList();

    public void CollectExperience()
    {
        var sfx = FMODEventReferenceDatabase.Load().collect_experience;
        FMODController.Instance.PlayWithLimitDelay(sfx);
    }
}