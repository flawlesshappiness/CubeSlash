using UnityEngine;

public class MinMaxFloat : MinMaxValue<float>
{
    protected override void ClampValue() => _value = Mathf.Clamp(_value, _min, _max);
    protected override bool IsEqual(float a, float b) => a == b;
}