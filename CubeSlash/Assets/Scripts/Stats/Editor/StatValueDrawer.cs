using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StatValue))]
public class StatValueDrawer : PropertyDrawer
{
    private SerializedProperty type_value;
    private SerializedProperty type_display;
    private SerializedProperty value_int;
    private SerializedProperty value_float;
    private SerializedProperty value_bool;

    private const float ELEMENT_HEIGHT = 20;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);

        type_value = property.FindPropertyRelative(nameof(type_value));
        type_display = property.FindPropertyRelative(nameof(type_display));
        value_int = property.FindPropertyRelative(nameof(value_int));
        value_float = property.FindPropertyRelative(nameof(value_float));
        value_bool = property.FindPropertyRelative(nameof(value_bool));

        EditorGUI.BeginChangeCheck();

        var rect_indended = EditorGUI.IndentedRect(position);
        var indent = rect_indended.x - position.x;
        var rect_popup_value = new Rect(rect_indended.x, position.y, EditorGUIUtility.labelWidth, ELEMENT_HEIGHT);
        var rect_value = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - rect_popup_value.width, ELEMENT_HEIGHT);

        var v_type_value = (StatValue.ValueType)EditorGUI.EnumPopup(rect_popup_value, (StatValue.ValueType)type_value.enumValueIndex);
        type_value.enumValueIndex = (int)v_type_value;

        if(v_type_value == StatValue.ValueType.INT)
        {
            value_int.intValue = EditorGUI.IntField(rect_value, value_int.intValue);
        }
        else if (v_type_value == StatValue.ValueType.FLOAT || v_type_value == StatValue.ValueType.PERCENT)
        {
            value_float.floatValue = EditorGUI.FloatField(rect_value, value_float.floatValue);
        }
        else if (v_type_value == StatValue.ValueType.BOOL)
        {
            value_bool.boolValue = EditorGUI.Toggle(rect_value, value_bool.boolValue);
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return ELEMENT_HEIGHT;
    }
}