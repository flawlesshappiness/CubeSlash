using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    public SpawnObjectInfo Info { get; set; }
    public Area Area { get; set; }

    public virtual void Initialize()
    {

    }

    public virtual void DestroySpawnObject()
    {
        ObjectSpawnController.Instance.OnObjectDestroyed(this);
        Destroy(gameObject);
    }

    private void Update()
    {
        RepositionUpdate();
    }

    public void RepositionUpdate()
    {
        if (Player.Instance == null) return;

        var size = GameSettings.Instance.area_size;
        var sh = size * 0.5f;
        var center = Player.Instance.transform.position;
        var pos = transform.position;

        if (pos.x < center.x - sh) transform.position += new Vector3(size, 0);
        if (pos.x > center.x + sh) transform.position -= new Vector3(size, 0);
        if (pos.y < center.y - sh) transform.position += new Vector3(0, size);
        if (pos.y > center.y + sh) transform.position -= new Vector3(0, size);
    }
}