using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class AbilityValue
{
    private int value_int;
    private float value_float;
    private bool value_bool;

    public AbilityValue(AbilityVariable variable)
    {
        AddValue(variable);
    }

    public AbilityValue(int i, float f, bool b)
    {

    }

    public void AddValue(AbilityVariable variable)
    {
        switch (variable.type_value)
        {
            case AbilityVariable.ValueType.INT:
                value_int += variable.value_int;
                break;

            case AbilityVariable.ValueType.FLOAT:
                value_float += variable.value_float;
                break;

            case AbilityVariable.ValueType.BOOL:
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