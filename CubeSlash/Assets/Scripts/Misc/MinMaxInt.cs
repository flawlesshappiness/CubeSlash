using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinMaxInt : MinMaxValue<int>
{
    protected override void ClampValue() => _value = Mathf.Clamp(_value, _min, _max);
    protected override bool IsEqual(int a, int b) => a == b;
}
