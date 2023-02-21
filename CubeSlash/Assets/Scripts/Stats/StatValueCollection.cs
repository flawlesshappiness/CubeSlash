using System.Collections.Generic;

public class StatValueCollection
{
    private Dictionary<StatID, StatValue> values = new Dictionary<StatID, StatValue>();
    private StatDatabase database;

	public StatValueCollection()
	{
        database = Database.Load<StatDatabase>();
	}

    public void ResetValues()
    {
        values.Clear();
        foreach(var stat in database.collection)
        {
            var value = new StatValue(stat.value);
            values.Add(stat.id, value);
        }
    }

    public StatValue GetValue(StatID id)
    {
        if(values.TryGetValue(id, out var value))
        {
            return value;
        }
        return null;
    }

    public void ApplyUpgrade(Upgrade upgrade)
    {
        foreach (var stat in upgrade.stats)
        {
            var value = GetValue(stat.id);
            value.AddValue(stat.value);
        }
    }

    public int GetIntValue(StatID id)
    {
        var value = GetValue(id);
        if(value.type_value != StatValue.ValueType.INT)
        {
            LogController.Instance.LogMessage($"StatValueCollection.GetIntValue({id}): Incorrect type. Value type is actually {value.type_value}");
        }
        return value.GetIntValue();
    }
    public float GetFloatValue(StatID id)
    {
        var value = GetValue(id);
        if (value.type_value != StatValue.ValueType.FLOAT)
        {
            LogController.Instance.LogMessage($"StatValueCollection.GetIntValue({id}): Incorrect type. Value type is actually {value.type_value}");
        }
        return value.GetFloatValue();
    }
    public bool GetBoolValue(StatID id)
    {
        var value = GetValue(id);
        if (value.type_value != StatValue.ValueType.BOOL)
        {
            LogController.Instance.LogMessage($"StatValueCollection.GetIntValue({id}): Incorrect type. Value type is actually {value.type_value}");
        }
        return value.GetBoolValue();
    }
}