using UnityEngine;

[System.Serializable]
public class StatValue
{
    private StatParameter.ValueType type_value;
    private StatParameter.DisplayType type_display;

    private int value_int;
    private float value_float;
    private bool value_bool;

    public StatValue(StatParameter variable)
    {
        AddValue(variable);
    }

    public string GetValueString() => StatParameter.GetValueString(value_int, value_float, value_bool, type_value, type_display, false);

    public void AddValue(StatParameter variable)
    {
        type_value = variable.type_value;
        type_display = variable.type_display;

        switch (variable.type_value)
        {
            case StatParameter.ValueType.INT:
                value_int += variable.value_int;
                break;

            case StatParameter.ValueType.FLOAT:
                value_float += variable.value_float;
                break;

            case StatParameter.ValueType.BOOL:
                value_bool = variable.value_bool;
                break;
        }
    }

    public void AddValue(int value) => value_int += value;
    public void AddValue(float value) => value_float += value;
    public void AddValue(bool value) => value_bool = value;

    public int GetIntValue() => value_int;
    public float GetFloatValue() => value_float;
    public bool GetBoolValue() => value_bool;

    public bool ComparePositiveTo(StatParameter parameter)
    {
        switch (parameter.type_value)
        {
            case StatParameter.ValueType.INT:
                return parameter.higher_is_positive ? value_int >= parameter.value_int : value_int <= parameter.value_int;

            case StatParameter.ValueType.FLOAT:
                return parameter.higher_is_positive ? value_float >= parameter.value_float : value_float <= parameter.value_float;

            case StatParameter.ValueType.BOOL:
                return parameter.higher_is_positive == parameter.value_bool;

            default: return false;
        }
    }
}