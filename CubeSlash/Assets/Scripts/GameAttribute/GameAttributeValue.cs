[System.Serializable]
public class GameAttributeValue
{
    public enum ValueType { Int, Float, Bool }
    public ValueType value_type;

    public int int_value;
    public float float_value;
    public bool bool_value;

    public GameAttributeValue()
    {

    }

    public GameAttributeValue(GameAttributeValue base_value)
    {
        value_type = base_value.value_type;
        int_value = base_value.int_value;
        float_value = base_value.float_value;
        bool_value = base_value.bool_value;
    }

    public string GetStringValue() => value_type switch
    {
        ValueType.Int => int_value.ToString(),
        ValueType.Float => float_value.ToString("0.##"),
        ValueType.Bool => bool_value.ToString(),
        _ => int_value.ToString(),
    };
}