using Flawliz.GenericOptions;
using UnityEngine;

public abstract class LeftRightIntControl : MonoBehaviour
{
    [SerializeField] private LeftRightControl _control;

    protected LeftRightControl Control => _control;
    protected abstract int Min { get; }
    protected abstract int Max { get; }

    private int _value;

    public System.Action<int> OnValueChanged;
    public System.Action<float> OnPercentageChanged;

    protected virtual void Awake()
    {
        _control.OnLeft += OnLeft;
        _control.OnRight += OnRight;
    }

    protected virtual void OnValidate()
    {
        _control ??= GetComponentInChildren<LeftRightControl>();
    }

    private void OnRight()
    {
        AdjustValue(1);
    }

    private void OnLeft()
    {
        AdjustValue(-1);
    }

    private void AdjustValue(int i)
    {
        SetValue(_value + i);
    }

    public void SetValue(int i)
    {
        _value = Mathf.Clamp(i, Min, Max);
        var t = GetPercentage();

        OnPercentageChanged?.Invoke(t);
        OnValueChanged?.Invoke(_value);
    }

    public void SetPercentage(float percentage)
    {
        var value = (int)(percentage * (Max - Min) + Min);
        SetValue(value);
    }

    public int GetSelectedValue() => Mathf.Clamp(_value, Min, Max);

    public float GetPercentage() => Mathf.Clamp01((float)(_value - Min) / (Max - Min));
}