using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Health
{
    private Dictionary<HealthPoint.Type, List<HealthPoint>> points = new Dictionary<HealthPoint.Type, List<HealthPoint>>();

    public event System.Action<HealthPoint> onAddHealthPoint;
    public event System.Action onDeath;

    public void Clear()
    {
        points.Values.SelectMany(points => points).ToList().ForEach(point => point.Destroy());
        points.Clear();
    }

    public void AddHealth(HealthPoint.Type type)
    {
        var hp = new HealthPoint { HealthType = type };
        var list = GetHealthList(type);
        list.Add(hp);
        onAddHealthPoint?.Invoke(hp);
    }

    public bool IsDead()
    {
        return GetHealthList(HealthPoint.Type.FULL).Count == 0 && GetHealthList(HealthPoint.Type.TEMPORARY).Count == 0;
    }

    public void Damage()
    {
        if (HasHealth(HealthPoint.Type.TEMPORARY))
        {
            var list_temp = GetHealthList(HealthPoint.Type.TEMPORARY);
            var hp = list_temp.Last();
            hp.Destroy();
            list_temp.Remove(hp);
        }
        else if(HasHealth(HealthPoint.Type.FULL))
        {
            var list_full = GetHealthList(HealthPoint.Type.FULL);
            var list_empty = GetHealthList(HealthPoint.Type.EMPTY);
            var hp = list_full.Last();
            list_full.Remove(hp);
            list_empty.Insert(0, hp);
            hp.Empty();
        }
    }

    public void Heal()
    {
        if (HasHealth(HealthPoint.Type.EMPTY))
        {
            var list_empty = GetHealthList(HealthPoint.Type.EMPTY);
            var list_full = GetHealthList(HealthPoint.Type.FULL);
            var hp = list_empty.First();
            list_full.Add(hp);
            hp.Fill();
        }
    }

    public bool HasHealth(HealthPoint.Type type) => points.ContainsKey(type) && points[type].Count > 0;
    public List<HealthPoint> GetHealthList(HealthPoint.Type type)
    {
        if (!points.ContainsKey(type))
            points.Add(type, new List<HealthPoint>());
        return points[type];
    }
}