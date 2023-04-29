using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BodypartController : Singleton
{
    public static BodypartController Instance { get { return Instance<BodypartController>(); } }

    private BodypartDatabase _db;

    protected override void Initialize()
    {
        base.Initialize();
        _db = Database.Load<BodypartDatabase>();
    }

    public BodypartInfo GetInfo(BodypartType type) => _db.collection.FirstOrDefault(item => item.type == type);
    public List<BodypartInfo> GetInfos() => _db.collection.ToList();
    public Bodypart CreateBodypart(BodypartType type)
    {
        var info = GetInfo(type);
        var bdp = Instantiate(info.prefab);
        bdp.Info = info;
        return bdp;
    }
}