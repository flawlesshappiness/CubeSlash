using UnityEngine;
using UnityEditor;
using Flawliz.Node;

[CustomPropertyDrawer(typeof(UpgradeNodeTree), true)]
public class UpgradeNodeTreePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var tree = fieldInfo.GetValue(property.serializedObject.targetObject) as UpgradeNodeTree;
        if (GUI.Button(position, label.text + " (Node Editor)"))
        {
            UpgradeNodeTreeWindow.Open(tree);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label);
    }
}