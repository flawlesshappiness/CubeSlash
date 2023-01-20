using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawnController : Singleton
{
    public static ObjectSpawnController Instance { get { return Instance<ObjectSpawnController>(); } }

    private List<SpawnObject> objects = new List<SpawnObject>();
    private List<Coroutine> cr_spawns = new List<Coroutine>();

    private Area current_area;

    protected override void Initialize()
    {
        base.Initialize();
        GameController.Instance.OnNextLevel += OnNextLevel;
        GameController.Instance.OnMainMenu += Clear;
    }

    private void OnNextLevel()
    {
        var area = Level.Current.area;
        if (current_area == area) return;
        current_area = area;

        ClearCoroutines();

        foreach(var info in area.spawn_objects)
        {
            for (int i = 0; i < info.count; i++)
            {
                var time = Random.Range(info.delay * 0.25f, info.delay);
                var cr = StartCoroutine(SpawnObjectCr(info, time));
                cr_spawns.Add(cr);

                StartCoroutine(RemoveWhenFinishedCr(cr));
            }
        }
    }

    public void OnObjectDestroyed(SpawnObject obj)
    {
        objects.Remove(obj);
        if (obj.Area != current_area) return;

        var cr = StartCoroutine(SpawnObjectCr(obj.Info, obj.Info.delay));
        cr_spawns.Add(cr);
        StartCoroutine(RemoveWhenFinishedCr(cr));
    }

    IEnumerator SpawnObjectCr(SpawnObjectInfo info, float delay)
    {
        yield return new WaitForSeconds(delay);

        var inst = Instantiate(info.prefab, GameController.Instance.world);
        inst.transform.position = CameraController.Instance.GetPositionOutsideCamera();
        inst.Info = info;
        inst.Area = current_area;
        inst.Initialize();
        objects.Add(inst);
    }

    IEnumerator RemoveWhenFinishedCr(Coroutine cr)
    {
        yield return cr;
        cr_spawns.Remove(cr);
    }

    private void ClearCoroutines()
    {
        foreach(var cr in cr_spawns)
        {
            StopCoroutine(cr);
        }
        cr_spawns.Clear();
    }

    private void ClearObjects()
    {
        foreach(var obj in objects)
        {
            Destroy(obj.gameObject);
        }
        objects.Clear();
    }

    public void Clear()
    {
        ClearCoroutines();
        ClearObjects();
    }
}