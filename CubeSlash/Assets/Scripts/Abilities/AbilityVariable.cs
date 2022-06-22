using UnityEngine;

public class AbilityVariable
{
    public int Max { get; private set; }
    public int Value { get; private set; }
    public float Percentage { get { return Value / (float)Max; } }

    public AbilityVariable(int value, int max)
    {
        Value = value;
        Max = max;
    }

    public void SetValue(int value)
    {
        Value = Mathf.Clamp(value, 0, Max);
    }
}