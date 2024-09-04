using UnityEditor;
using UnityEngine;

namespace Narazaka.VRChat.AvatarStatusWindowMaker.Editor
{
    [CustomPropertyDrawer(typeof(AvatarStatus))]
    public class AvatarStatusPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var name = property.FindPropertyRelative(nameof(AvatarStatus.name));
            var min = property.FindPropertyRelative(nameof(AvatarStatus.min));
            var max = property.FindPropertyRelative(nameof(AvatarStatus.max));
            var value = property.FindPropertyRelative(nameof(AvatarStatus.value));
            var menu = property.FindPropertyRelative(nameof(AvatarStatus.menu));

            var x = position.x;
            var width = position.width;

            EditorGUIUtility.labelWidth = 35;
            var rect = new Rect(x, position.y, width - 250, position.height);
            EditorGUI.PropertyField(rect, name);
            rect.x += rect.width + 5;
            rect.width = 40;
            EditorGUI.PropertyField(rect, value, GUIContent.none);
            EditorGUIUtility.labelWidth = 25;
            rect.x += rect.width + 5;
            rect.width = 65;
            EditorGUI.PropertyField(rect, min);
            rect.x += rect.width + 5;
            EditorGUI.PropertyField(rect, max);
            EditorGUIUtility.labelWidth = 35;
            rect.x += rect.width + 5;
            EditorGUI.PropertyField(rect, menu);

            EditorGUIUtility.labelWidth = 0;

            EditorGUI.EndProperty();

            if (max.floatValue == 0)
            {
                max.floatValue = 100;
            }
            if (min.floatValue < 0)
            {
                min.floatValue = 0;
            }
            if (min.floatValue > max.floatValue)
            {
                min.floatValue = max.floatValue;
            }
            if (value.floatValue < min.floatValue)
            {
                value.floatValue = min.floatValue;
            }
            if (value.floatValue > max.floatValue)
            {
                value.floatValue = max.floatValue;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
