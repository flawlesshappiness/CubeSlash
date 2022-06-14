using UnityEngine;

public class ExperienceController : Singleton
{
    public static ExperienceController Instance { get { return Singleton.Instance<ExperienceController>(); } }

    private Experience prefab_plant;
    private Experience prefab_meat;

    private void Start()
    {
        prefab_plant = Resources.Load<Experience>("Prefabs/Experience/Plant");
        prefab_meat = Resources.Load<Experience>("Prefabs/Experience/Meat");
    }

    public Experience SpawnMeat()
    {
        return SpawnExperience(prefab_meat);
    }

    public Experience SpawnPlant()
    {
        return SpawnExperience(prefab_plant);
    }

    private Experience SpawnExperience(Experience prefab)
    {
        var e = Instantiate(prefab, GameController.Instance.world).GetComponent<Experience>();
        return e;
    }
}