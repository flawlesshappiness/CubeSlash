using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameAttributeValue))]
public class GameAttributeValueDrawer : PropertyDrawer
{
    private SerializedProperty value_type;
    private SerializedProperty int_value;
    private SerializedProperty float_value;
    private SerializedProperty bool_value;

    private float height;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);

        // Find properties
        value_type = property.FindPropertyRelative(nameof(value_type));
        int_value = property.FindPropertyRelative(nameof(int_value));
        float_value = property.FindPropertyRelative(nameof(float_value));
        bool_value = property.FindPropertyRelative(nameof(bool_value));

        var type = (GameAttributeValue.ValueType)value_type.enumValueIndex;
        var value_property = type switch
        {
            GameAttributeValue.ValueType.Int => int_value,
            GameAttributeValue.ValueType.Float => float_value,
            GameAttributeValue.ValueType.Bool => bool_value,
            _ => int_value
        };

        var property_rect = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(value_type));
        EditorGUI.PropertyField(property_rect, value_type, false);
        property_rect.y += property_rect.height + 1;
        EditorGUI.PropertyField(property_rect, value_property, false);
        property_rect.y += property_rect.height + 1;

        height = property_rect.y - position.y;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return height;
    }
}