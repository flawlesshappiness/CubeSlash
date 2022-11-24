using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class StatValueCollection
{
    private Dictionary<string, StatValue> values = new Dictionary<string, StatValue>();
	private StatCollection stats;

	public StatValueCollection(StatCollection stats)
	{
		this.stats = stats;
	}

    public void ResetValues()
    {
        values.Clear();
        foreach (var v in stats.stats)
        {
            var value = new StatValue(v);
            values.Add(v.name, value);
        }
    }

    public StatValue GetValue(string id)
    {
        if(values.TryGetValue(id, out var value))
        {
            return value;
        }
        return null;
    }

    public void ApplyEffects(List<Upgrade.Effect> effects)
    {
        foreach (var effect in effects)
        {
            var value = GetValue(effect.variable.name);
            value.AddValue(effect.variable);
        }
    }

    public int GetIntValue(string name) => GetValue(name).GetIntValue();
    public float GetFloatValue(string name) => GetValue(name).GetFloatValue();
    public bool GetBoolValue(string name) => GetValue(name).GetBoolValue();
    public void AddValue(string name, int value) => GetValue(name).AddValue(value);
    public void AddValue(string name, float value) => GetValue(name).AddValue(value);
    public void AddValue(string name, bool value) => GetValue(name).AddValue(value);
}