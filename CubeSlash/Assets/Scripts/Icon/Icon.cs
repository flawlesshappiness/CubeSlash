using System.Linq;
using UnityEngine;

public static class Icon
{
    private static IconDatabase _db;
    public static IconDatabase DB { get { return _db ?? (_db = Database.Load<IconDatabase>()); } }

    public static Sprite Get(IconType type)
    {
        var info = DB.collection.FirstOrDefault(item => item.type == type);
        if (info == null) return null;

        return info.sprite;
    }
}