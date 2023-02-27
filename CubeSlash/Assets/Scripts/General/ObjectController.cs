using System.Collections.Generic;
using UnityEngine;

public class ObjectController : Singleton
{
    public static ObjectController Instance { get { return Instance<ObjectController>(); } }

    private List<GameObject> gameObjects = new List<GameObject>();

    protected override void Initialize()
    {
        base.Initialize();
        GameController.Instance.onMainMenu += Clear;
    }

    public void Add(GameObject gameObject)
    {
        gameObjects.Add(gameObject);
    }

    public void Remove(GameObject gameObject)
    {
        gameObjects.Remove(gameObject);
    }

    public void Clear()
    {
        foreach(var gameObject in gameObjects)
        {
            Destroy(gameObject);
        }
        gameObjects.Clear();
    }
}