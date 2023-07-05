using TMPro;
using UnityEngine;

public class UIFloatingPanelUpgradeFromToText : MonoBehaviour
{
    [SerializeField] private TMP_Text tmp_from, tmp_to, tmp_arrow;
    [SerializeField] private ColorPaletteValue neutral_color, positive_color, negative_color;

    public enum ColorType { Neutral, Positive, Negative }

    public void SetFromText(string text)
    {
        tmp_from.text = text;
    }

    public void SetToText(string text)
    {
        tmp_to.text = text;
    }

    public void SetToColor(Color color)
    {
        tmp_to.color = color;
    }

    public void SetToColor(ColorType type)
    {
        var color = type switch
        {
            ColorType.Positive => positive_color.color,
            ColorType.Negative => negative_color.color,
            _ => neutral_color.color,
        };
        SetToColor(color);
    }

    public void SetTextFromAttributeModifier(GameAttribute attribute, GameAttributeModifier modifier)
    {
        var from_value = new GameAttributeValue(attribute.ModifiedValue);

        var to_attribute = attribute.Clone();
        to_attribute.AddModifier(modifier);
        var to_value = to_attribute.ModifiedValue;

        SetFromText(from_value.GetStringValue());
        SetToText(to_value.GetStringValue());

        var color = GetColorType(from_value, to_value, attribute.high_is_negative);
        SetToColor(color);
    }

    private ColorType GetColorType(GameAttributeValue from_value, GameAttributeValue to_value, bool high_is_negative)
    {
        var type = to_value.value_type switch
        {
            GameAttributeValue.ValueType.Int => to_value.int_value > from_value.int_value ? ColorType.Positive : to_value.int_value == from_value.int_value ? ColorType.Neutral : ColorType.Negative,
            GameAttributeValue.ValueType.Float => to_value.float_value > from_value.float_value ? ColorType.Positive : to_value.float_value == from_value.float_value ? ColorType.Neutral : ColorType.Negative,
            GameAttributeValue.ValueType.Bool => to_value.bool_value && !from_value.bool_value ? ColorType.Positive : to_value.bool_value == from_value.bool_value ? ColorType.Neutral : ColorType.Negative,
            _ => ColorType.Neutral,
        };

        if (high_is_negative)
        {
            type = type switch
            {
                ColorType.Positive => ColorType.Negative,
                ColorType.Negative => ColorType.Positive,
                _ => type,
            };
        }

        return type;
    }
}