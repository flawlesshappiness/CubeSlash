using System.Collections.Generic;

public class GameAttributeController : Singleton
{
    public static GameAttributeController Instance { get { return Instance<GameAttributeController>(); } }

    private Dictionary<GameAttributeType, GameAttribute> attributes = new Dictionary<GameAttributeType, GameAttribute>();

    protected override void Initialize()
    {
        AddAttributesFromDB();
    }

    private void AddAttributesFromDB()
    {
        var db = Database.Load<GameAttributeDatabase>();
        foreach (var item in db.collection)
        {
            var att = item.attribute.Clone();
            attributes.Add(att.type, att);
        }
    }

    public void Clear()
    {
        attributes.Clear();
        AddAttributesFromDB();
    }

    public GameAttribute GetAttribute(GameAttributeType type) => attributes[type];
}