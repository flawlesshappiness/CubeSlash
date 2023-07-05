using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameAttributeModifier))]
public class GameAttributeModifierDrawer : PropertyDrawer
{
    private SerializedProperty attribute_type;
    private SerializedProperty value_type;
    private SerializedProperty int_value;
    private SerializedProperty float_value;
    private SerializedProperty percent_value;
    private SerializedProperty bool_value;

    private float height;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);

        // Find properties
        attribute_type = property.FindPropertyRelative(nameof(attribute_type));
        value_type = property.FindPropertyRelative(nameof(value_type));
        int_value = property.FindPropertyRelative(nameof(int_value));
        float_value = property.FindPropertyRelative(nameof(float_value));
        percent_value = property.FindPropertyRelative(nameof(percent_value));
        bool_value = property.FindPropertyRelative(nameof(bool_value));

        var type = (GameAttributeModifier.ValueType)value_type.enumValueIndex;
        var value_property = type switch
        {
            GameAttributeModifier.ValueType.Int => int_value,
            GameAttributeModifier.ValueType.Float => float_value,
            GameAttributeModifier.ValueType.Percent => percent_value,
            GameAttributeModifier.ValueType.Bool => bool_value,
            _ => int_value
        };

        var spacing = 1f;
        var property_rect = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(attribute_type));
        EditorGUI.PropertyField(property_rect, attribute_type, false);
        property_rect.y += property_rect.height + spacing;
        EditorGUI.PropertyField(property_rect, value_type, false);
        property_rect.y += property_rect.height + spacing;
        EditorGUI.PropertyField(property_rect, value_property, false);
        property_rect.y += property_rect.height + spacing;

        height = property_rect.y - position.y;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return height;
    }
}