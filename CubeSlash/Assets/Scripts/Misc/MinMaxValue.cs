using UnityEngine;

public abstract class MinMaxValue<T>
{
    protected T _min;
    protected T _max;
    protected T _value;
    public T Min { get { return _min; } set { SetMin(value); } }
    public T Max { get { return _max; } set { SetMax(value); } }
    public T Value { get { return _value; } set { SetValue(value); } }

    public System.Action onMin;
    public System.Action onMax;
    public System.Action onValueChanged;

    protected abstract void ClampValue();
    protected abstract bool IsEqual(T a, T b);

    public void SetMin(T min)
    {
        _min = min;
        ClampValue();
    }

    public void SetMax(T max)
    {
        _max = max;
        ClampValue();
    }

    public void SetValue(T value)
    {
        _value = value;
        ClampValue();

        onValueChanged?.Invoke();

        if (IsEqual(Value, Min))
        {
            onMin?.Invoke();
        }
        else if (IsEqual(Value, Max))
        {
            onMax?.Invoke();
        }
    }
}