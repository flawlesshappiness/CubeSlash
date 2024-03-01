using System.Collections.Generic;
using System.Linq;

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
        LogController.LogMethod(type.id);

        var info = GetInfo(type);
        var bdp = Instantiate(info.prefab);
        bdp.Info = info;
        return bdp;
    }

    public void UnlockPart(BodypartInfo info)
    {
        if (Save.Game.unlocked_bodyparts.Contains(info.type)) return;
        Save.Game.unlocked_bodyparts.Add(info.type);
        Save.Game.new_bodyparts.Add(info.type);
    }

    public BodypartInfo UnlockRandomPart()
    {
        var parts = _db.collection.Where(info => !info.is_ability_part && !Save.Game.unlocked_bodyparts.Contains(info.type));
        if (parts.Count() > 0)
        {
            var info = parts.ToList().Random();
            UnlockPart(info);
            return info;
        }

        return null;
    }

    public void UnlockAllParts()
    {
        var parts = _db.collection.Where(info => !info.is_ability_part && !Save.Game.unlocked_bodyparts.Contains(info.type));
        parts.ToList().ForEach(info => UnlockPart(info));
    }
}