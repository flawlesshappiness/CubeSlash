using System.Collections.Generic;
using System.Linq;

public class Health
{
    private Dictionary<HealthPoint.Type, List<HealthPoint>> points = new Dictionary<HealthPoint.Type, List<HealthPoint>>();

    public bool ResetUI { get; private set; }
    private bool HealthConvertedToArmor { get; set; }

    public event System.Action<HealthPoint> onAddHealthPoint;
    public event System.Action onDeath;

    public void Clear()
    {
        points.Values.SelectMany(points => points).ToList().ForEach(point => point.Destroy());
        points.Clear();
    }

    public void AddHealth(HealthPoint.Type type)
    {
        if (HealthConvertedToArmor && (type == HealthPoint.Type.FULL || type == HealthPoint.Type.EMPTY))
        {
            AddHealth(HealthPoint.Type.TEMPORARY);
            AddHealth(HealthPoint.Type.TEMPORARY);
            return;
        }

        var hp = new HealthPoint { HealthType = type };
        var list = GetHealthList(type);
        list.Add(hp);
        onAddHealthPoint?.Invoke(hp);
    }

    public void ConvertHealthToArmor()
    {
        HealthConvertedToArmor = true;

        var full_health = GetHealthList(HealthPoint.Type.FULL);
        var empty_health = GetHealthList(HealthPoint.Type.EMPTY);
        var count = full_health.Count + empty_health.Count;
        full_health.ToList().ForEach(hp => full_health.Remove(hp));
        empty_health.ToList().ForEach(hp => empty_health.Remove(hp));
        for (int i = 0; i < count * 2; i++)
        {
            AddHealth(HealthPoint.Type.TEMPORARY);
        }

        ResetUI = true;
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
        else if (HasHealth(HealthPoint.Type.FULL))
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
            list_empty.Remove(hp);
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

    public void OnUIReset()
    {
        ResetUI = false;
    }
}