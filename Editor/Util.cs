using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Narazaka.VRChat.AvatarStatusWindowMaker.Editor
{
    public static class Util
    {
        static string statusLinePrefabPath = "Packages/net.narazaka.vrchat.avatar-status-window-maker/Assets/StatusLine.prefab";
        static string valuePrefabPath = "Packages/net.narazaka.vrchat.avatar-status-window-maker/Assets/ValueModel.prefab";
        static string statusValueMaterialPath = "Packages/net.narazaka.vrchat.avatar-status-window-maker/Assets/StatusValue.mat";
        static string animatorPath = "Packages/net.narazaka.vrchat.avatar-status-window-maker/Assets/NumberRate.controller";

        static GameObject _statusLinePrefab;
        public static GameObject statusLinePrefab
        {
            get
            {
                if (_statusLinePrefab == null) _statusLinePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(statusLinePrefabPath);
                return _statusLinePrefab;
            }
        }

        static GameObject _valuePrefab;
        public static GameObject valuePrefab
        {
            get
            {
                if (_valuePrefab == null) _valuePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(valuePrefabPath);
                return _valuePrefab;
            }
        }

        static Material _statusValueMaterial;
        public static Material statusValueMaterial
        {
            get
            {
                if (_statusValueMaterial == null) _statusValueMaterial = AssetDatabase.LoadAssetAtPath<Material>(statusValueMaterialPath);
                return _statusValueMaterial;
            }
        }

        static AnimatorController _animator;
        public static AnimatorController animator
        {
            get
            {
                if (_animator == null) _animator = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorPath);
                return _animator;
            }
        }

        public static string ChildPath(GameObject baseObject, GameObject targetObject)
        {
            var paths = new List<string>();
            var transform = targetObject.transform;
            var baseObjectTransform = baseObject == null ? null : baseObject.transform;
            while (baseObjectTransform != transform && transform != null)
            {
                paths.Add(transform.gameObject.name);
                transform = transform.parent;
            }
            paths.Reverse();
            return string.Join("/", paths.ToArray());
        }
    }
}
