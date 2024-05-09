public class SpawnObject : WorldObject
{
    public SpawnObjectInfo Info { get; set; }

    public virtual void Initialize()
    {

    }

    public void Destroy()
    {
        ObjectSpawnController.Instance.OnObjectDestroyed(this);
        Destroy(gameObject);
    }
}