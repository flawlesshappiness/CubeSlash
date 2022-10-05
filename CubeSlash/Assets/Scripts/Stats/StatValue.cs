using UnityEngine;

[System.Serializable]
public class StatValue
{
    private int value_int;
    private float value_float;
    private bool value_bool;

    public StatValue(StatParameter variable)
    {
        AddValue(variable);
    }

    public StatValue(int i, float f, bool b)
    {

    }

    public void AddValue(StatParameter variable)
    {
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
}