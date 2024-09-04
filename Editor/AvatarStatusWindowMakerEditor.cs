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
        MeshRenderer mesh;
        Camera camera;
        Canvas canvas;
        Text titleUI;
        GameObject statusLinesContainer;
        //GameObject valuesContainer;

        private void OnEnable()
        {
            displayName = serializedObject.FindProperty(nameof(AvatarStatusWindowMaker.displayName));
            size = serializedObject.FindProperty(nameof(AvatarStatusWindowMaker.size));
            statuses = serializedObject.FindProperty(nameof(AvatarStatusWindowMaker.statuses));
            var avatarStatusWindowMaker = target as AvatarStatusWindowMaker;
            mesh = avatarStatusWindowMaker.transform.Find("Mesh").GetComponent<MeshRenderer>();
            camera = avatarStatusWindowMaker.transform.Find("Camera").GetComponent<Camera>();
            canvas = avatarStatusWindowMaker.transform.Find("Canvas").GetComponent<Canvas>();
            titleUI = canvas.transform.Find("Container/Title").GetComponent<Text>();
            statusLinesContainer = canvas.transform.Find("Container/StatusLines").gameObject;
            //valuesContainer = avatarStatusWindowMaker.transform.Find("Values").gameObject;
            UpdateElements();
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

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                UpdateElements();
            }
        }

        void UpdateElements()
        {
            canvas.GetComponent<RectTransform>().sizeDelta = size.vector2Value;
            titleUI.text = displayName.stringValue;
            var layout = statusLinesContainer.GetComponent<VerticalLayoutGroup>();
            if (layout == null) layout = statusLinesContainer.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.spacing = 0;
            layout.padding = new RectOffset(20, 20, 20, 0);
            var statusCount = statuses.arraySize;
            var statusUICount = statusLinesContainer.transform.childCount;
            //var valueUICount = valuesContainer.transform.childCount;
            for (var i = 0; i < statusCount; i++)
            {
                var status = statuses.GetArrayElementAtIndex(i);
                var name = status.FindPropertyRelative(nameof(AvatarStatus.name)).stringValue;
                var min = status.FindPropertyRelative(nameof(AvatarStatus.min)).floatValue;
                var max = status.FindPropertyRelative(nameof(AvatarStatus.max)).floatValue;
                var value = status.FindPropertyRelative(nameof(AvatarStatus.value)).floatValue;
                GameObject statusLine;
                if (i < statusUICount)
                {
                    statusLine = statusLinesContainer.transform.GetChild(i).gameObject;
                }
                else
                {
                    statusLine = PrefabUtility.InstantiatePrefab(Util.statusLinePrefab) as GameObject;
                    statusLine.transform.SetParent(statusLinesContainer.transform, false);
                }
                statusLine.name = name;

                var nameUI = statusLine.transform.Find("Name").GetComponent<Text>();
                var valueUI = statusLine.transform.Find("Value/ValueModel").GetComponent<Renderer>();
                nameUI.text = name;
                if (valueUI.sharedMaterial == null || valueUI.sharedMaterial == Util.statusValueMaterial)
                {
                    valueUI.sharedMaterial = new Material(Util.statusValueMaterial);
                }
                valueUI.sharedMaterial.SetFloat("_MinNumber", min);
                valueUI.sharedMaterial.SetFloat("_MaxNumber", max);
                valueUI.sharedMaterial.SetFloat("_NumberRate", AvatarStatus.ValueRate(min, max, value));
                /*
                GameObject valueObj;
                if (i < valueUICount)
                {
                    valueObj = valuesContainer.transform.GetChild(i).gameObject;
                }
                else
                {
                    valueObj = PrefabUtility.InstantiatePrefab(Util.valuePrefab) as GameObject;
                    valueObj.transform.SetParent(valuesContainer.transform, false);
                    var pos = valueObj.AddComponent<PositionConstraint>();
                    pos.AddSource(new ConstraintSource { sourceTransform = valueUI.transform });
                    pos.constraintActive = true;
                    var rot = valueObj.AddComponent<RotationConstraint>();
                    rot.AddSource(new ConstraintSource { sourceTransform = valueUI.transform });
                    rot.constraintActive = true;
                }
                valueObj.name = name;
                valueUI = valueObj.GetComponent<Renderer>();
                if (valueUI.sharedMaterial == Util.statusValueMaterial)
                {
                    valueUI.sharedMaterial = new Material(valueUI.sharedMaterial);
                }
                valueUI.sharedMaterial.SetFloat("_MinNumber", min);
                valueUI.sharedMaterial.SetFloat("_MaxNumber", max);
                valueUI.sharedMaterial.SetFloat("_NumberRate", GetRate(min, max, value));
                */
            }
            for (var i = statusCount; i < statusUICount; i++)
            {
                var statusLine = statusLinesContainer.transform.GetChild(i).gameObject;
                DestroyImmediate(statusLine.transform.Find("Value/ValueModel").GetComponent<Renderer>().sharedMaterial);
                DestroyImmediate(statusLine);
            }
            /*
            for (var i = statusCount; i < valueUICount; i++)
            {
                var valueObj = valuesContainer.transform.GetChild(i).gameObject;
                DestroyImmediate(valueObj.GetComponent<Renderer>().sharedMaterial);
                DestroyImmediate(valueObj);
            }
            */
            // Object.DestroyImmediate(layout);
            SynchronizationContext.Current.Send(async state =>
            {
                await Task.Delay(100);
                Debug.Log("DestroyImmediate");
                Debug.Log(statusLinesContainer);
                var layout = statusLinesContainer.GetComponent<VerticalLayoutGroup>();
                Debug.Log(layout);
                if (layout != null) DestroyImmediate(layout);
            }, null);

            var renderTextureSize = AvatarStatusWindowMaker.RenderTextureSize(size.vector2Value);
            if (camera.targetTexture == null || camera.targetTexture.width != renderTextureSize.x || camera.targetTexture.height != renderTextureSize.y)
            {
                if (camera.targetTexture != null)
                {
                    var toDestroy = camera.targetTexture;
                    camera.targetTexture = null;
                    DestroyImmediate(toDestroy);
                }
                camera.targetTexture = new RenderTexture(renderTextureSize.x, renderTextureSize.y, 24);
            }

            var active = RenderTexture.active;
            RenderTexture.active = camera.targetTexture;
            camera.Render();
            var resizedRenderTexture = new RenderTexture(1024, 1024, 24);
            RenderTexture.active = resizedRenderTexture;
            Graphics.Blit(camera.targetTexture, resizedRenderTexture);
            var texture = new Texture2D(resizedRenderTexture.width, resizedRenderTexture.height);
            texture.ReadPixels(new Rect(0, 0, resizedRenderTexture.width, resizedRenderTexture.height), 0, 0);
            RenderTexture.active = active;
            DestroyImmediate(resizedRenderTexture);

            mesh.transform.localScale = new Vector3(size.vector2Value.x / 1000, size.vector2Value.y / 1000, 1);
            if (mesh.sharedMaterial != null)
            {
                if (mesh.sharedMaterial.mainTexture != null)
                {
                    DestroyImmediate(mesh.sharedMaterial.mainTexture);
                }
                DestroyImmediate(mesh.sharedMaterial);
            }
            mesh.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
            mesh.sharedMaterial.mainTexture = texture;
        }
    }
}
