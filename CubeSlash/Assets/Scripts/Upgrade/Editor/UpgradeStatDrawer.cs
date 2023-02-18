using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(UpgradeStat))]
public class UpgradeStatDrawer : PropertyDrawer
{
    private const float ELEMENT_HEIGHT = 20;

    private SerializedProperty id;
    private SerializedProperty id_id;
    private SerializedProperty value;
    private SerializedProperty type_value;
    private SerializedProperty value_int;
    private SerializedProperty value_float;
    private SerializedProperty value_bool;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);

        id = property.FindPropertyRelative(nameof(id));
        id_id = id.FindPropertyRelative("id");
        value = property.FindPropertyRelative(nameof(value));
        type_value = value.FindPropertyRelative(nameof(type_value));
        value_int = value.FindPropertyRelative(nameof(value_int));
        value_float = value.FindPropertyRelative(nameof(value_float));
        value_bool = value.FindPropertyRelative(nameof(value_bool));

        var rect_indended = EditorGUI.IndentedRect(position);
        var rect_popup = new Rect(rect_indended.x, position.y, position.width, ELEMENT_HEIGHT);
        var rect_value = new Rect(position.x, position.y + ELEMENT_HEIGHT * 1, position.width, ELEMENT_HEIGHT);
        var rect_label = new Rect(position.x, position.y + ELEMENT_HEIGHT * 2, position.width, ELEMENT_HEIGHT);

        EditorGUI.BeginChangeCheck();

        EditorGUI.PropertyField(rect_popup, id);
        var ids = FakeEnum.GetAll(typeof(StatID));
        var v_id = (StatID)ids.FirstOrDefault(v => v.id == id_id.stringValue);
        var db_stat = Database.Load<StatDatabase>();
        var stat = db_stat.collection.FirstOrDefault(stat => stat.id == v_id);
        if (stat == null) stat = db_stat.collection[0];

        type_value.enumValueIndex = (int)stat.value.type_value;
        var v_type_value = (StatValue.ValueType)type_value.enumValueIndex;
        var p_value = v_type_value switch
        {
            StatValue.ValueType.INT => value_int,
            StatValue.ValueType.FLOAT => value_float,
            StatValue.ValueType.PERCENT => value_float,
            StatValue.ValueType.BOOL => value_bool,
        };
        EditorGUI.PropertyField(rect_value, p_value, new GUIContent($"Value ({stat.value.GetValueString()})"));

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return ELEMENT_HEIGHT * 2;
    }
}