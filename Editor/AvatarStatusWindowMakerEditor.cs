using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Narazaka.VRChat.AvatarStatusWindowMaker.Editor
{
    [CustomEditor(typeof(AvatarStatusWindowMaker))]
    [CanEditMultipleObjects]
    public class AvatarStatusWindowMakerEditor : UnityEditor.Editor
    {
        SerializedProperty displayName;
        SerializedProperty size;
        SerializedProperty statuses;
        SerializedProperty defaultActive;

        private void OnEnable()
        {
            displayName = serializedObject.FindProperty(nameof(AvatarStatusWindowMaker.displayName));
            size = serializedObject.FindProperty(nameof(AvatarStatusWindowMaker.size));
            statuses = serializedObject.FindProperty(nameof(AvatarStatusWindowMaker.statuses));
            defaultActive = serializedObject.FindProperty(nameof(AvatarStatusWindowMaker.defaultActive));
            (target as AvatarStatusWindowMaker).Render();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(displayName);
            EditorGUILayout.PropertyField(size);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var sizeX = EditorGUILayout.Slider("Size X", size.vector2Value.x, 0, 1000);
                if (check.changed) size.vector2Value = new Vector2(sizeX, size.vector2Value.y);
            }
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var sizeY = EditorGUILayout.Slider("Size Y", size.vector2Value.y, 0, 1000);
                if (check.changed) size.vector2Value = new Vector2(size.vector2Value.x, sizeY);
            }
            EditorGUILayout.PropertyField(statuses, true);
            EditorGUILayout.PropertyField(defaultActive);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            if (!(target as AvatarStatusWindowMaker).gameObject.activeInHierarchy)
            {
                EditorGUILayout.HelpBox("オブジェクトをアクティブにしてください。\nデフォルトで非アクティブにしたい場合はDefault Activeをオフにして下さい。", MessageType.Warning);
            }
        }
    }
}
