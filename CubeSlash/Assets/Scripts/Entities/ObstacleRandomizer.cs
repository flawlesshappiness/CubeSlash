using System.Collections.Generic;
using UnityEngine;

public class ObstacleRandomizer : MonoBehaviour
{
    [SerializeField] private List<GameObject> obstacles = new List<GameObject>();
    [SerializeField] private int size_min, size_max;
    [SerializeField] private bool randomize_rotation;

    private void Start()
    {
        if (obstacles.Count == 0) return;
        obstacles.ForEach(g => g.SetActive(false));
        var obstacle = obstacles.Random();
        obstacle.SetActive(true);
        transform.localScale = Vector3.one * Random.Range(size_min, size_max);

        var angle = randomize_rotation ? Random.Range(0f, 360f) : 0f;
        obstacle.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}