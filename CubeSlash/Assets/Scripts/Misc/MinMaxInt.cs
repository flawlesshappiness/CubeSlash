using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinMaxInt
{
    private int _min;
    private int _max;
    private int _value;
    public int Min { get { return _min; } set { SetMin(value); } }
    public int Max { get { return _max; } set { SetMax(value); } }
    public int Value { get { return _value; } set { SetValue(value); } }

    public System.Action onMin;
    public System.Action onMax;
    public System.Action onValueChanged;

    private void UpdateValue()
    {
        _value = Mathf.Clamp(_value, _min, _max);
    }

    private void SetMin(int value)
    {
        _min = value;
        UpdateValue();
    }

    private void SetMax(int value)
    {
        _max = value;
        UpdateValue();
    }

    private void SetValue(int value)
    {
        _value = value;
        UpdateValue();

        onValueChanged?.Invoke();

        if(Value == Min)
        {
            onMin?.Invoke();
        }
        else if(Value == Max)
        {
            onMax?.Invoke();
        }
    }
}
