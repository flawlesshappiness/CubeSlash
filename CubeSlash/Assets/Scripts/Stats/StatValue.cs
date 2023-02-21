using UnityEngine;

[System.Serializable]
public class StatValue
{
    public enum ValueType { INT, FLOAT, BOOL, PERCENT }
    public enum DisplayType { INT, FLOAT, PERCENT, TEXT }

    public ValueType type_value;
    public DisplayType type_display;

    public int value_int;
    public float value_float;
    public bool value_bool;

    public StatValue(StatValue value)
    {
        AddValue(value);
    }

    public StatValue()
    {

    }

    public void AddValue(StatValue value)
    {
        switch (value.type_value)
        {
            case ValueType.INT:
                AddValue(value.GetIntValue());
                break;

            case ValueType.FLOAT:
                AddValue(value.GetFloatValue());
                break;

            case ValueType.PERCENT:
                AddValue(value.GetFloatValue());
                break;
        }
    }

    public void AddValue(int value) => value_int += value;
    public void AddValue(float value) => value_float += value;
    public void AddValue(bool value) => value_bool = value;

    public int GetIntValue() => value_int;
    public float GetFloatValue() => value_float;
    public bool GetBoolValue() => value_bool;

    public bool IsPositive()
    {
        return type_value switch
        {
            ValueType.INT => value_int > 0,
            ValueType.FLOAT => value_float > 0,
            ValueType.PERCENT => value_float > 0,
            _ => true
        };
    }

    public static string GetValueString(int i, float f, ValueType type)
    {
        return type switch
        {
            ValueType.INT => $"{i}",
            ValueType.FLOAT => $"{f.ToString("0.##")}",
            ValueType.PERCENT => $"{Mathf.RoundToInt(f * 100f)}%",
            _ => "",
        };
    }

    public string GetValueString()
    {
        var i = value_int;
        var f = value_float;
        return GetValueString(i, f, type_value);
    }
}