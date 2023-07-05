[System.Serializable]
public class GameAttributeModifier
{
    public GameAttributeType attribute_type;

    public enum ValueType { Int, Float, Bool, Percent }
    public ValueType value_type;

    public int int_value;
    public float float_value;
    public float percent_value = 1f;
    public bool bool_value;

    public GameAttributeModifier Clone()
    {
        var modifier = new GameAttributeModifier();
        modifier.int_value = int_value;
        modifier.float_value = float_value;
        modifier.percent_value = percent_value;
        modifier.bool_value = bool_value;
        return modifier;
    }

    public void Add(GameAttributeModifier modifier)
    {
        switch (modifier.value_type)
        {
            case ValueType.Int:
                int_value += modifier.int_value;
                break;

            case ValueType.Float:
                float_value += modifier.float_value;
                break;

            case ValueType.Percent:
                percent_value += modifier.percent_value;
                break;

            case ValueType.Bool:
                bool_value = modifier.bool_value;
                break;
        }
    }

    public void Modify(GameAttributeValue value)
    {
        if (value.value_type == GameAttributeValue.ValueType.Int)
        {
            value.int_value += int_value;
            value.int_value += (int)float_value;
            value.int_value = (int)(value.int_value * percent_value);
        }
        else if (value.value_type == GameAttributeValue.ValueType.Float)
        {
            value.float_value += int_value;
            value.float_value += float_value;
            value.float_value *= percent_value;
        }
        else if (value.value_type == GameAttributeValue.ValueType.Bool)
        {
            value.bool_value = bool_value;
        }
    }

    public string GetValueString() => value_type switch
    {
        ValueType.Int => int_value.ToString(),
        ValueType.Float => float_value.ToString(),
        ValueType.Percent => $"{(int)(percent_value * 100)}%",
        _ => ""
    };
}