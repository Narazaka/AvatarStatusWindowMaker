using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Narazaka.VRChat.AvatarStatusWindowMaker
{
    internal class UpdateElements
    {
        AvatarStatusWindowMaker avatarStatusWindowMaker;
        MeshRenderer mesh;
        Camera camera;
        Canvas canvas;
        Text titleUI;
        GameObject statusLinesContainer;
        Vector2 size => avatarStatusWindowMaker.size;
        string displayName => avatarStatusWindowMaker.displayName;
        List<AvatarStatus> statuses => avatarStatusWindowMaker.statuses;

        public UpdateElements(AvatarStatusWindowMaker target)
        {
            avatarStatusWindowMaker = target;
            mesh = avatarStatusWindowMaker.transform.Find("Mesh").GetComponent<MeshRenderer>();
            camera = avatarStatusWindowMaker.transform.Find("Camera").GetComponent<Camera>();
            canvas = avatarStatusWindowMaker.transform.Find("Canvas").GetComponent<Canvas>();
            titleUI = canvas.transform.Find("Container/Title").GetComponent<Text>();
            statusLinesContainer = canvas.transform.Find("Container/StatusLines").gameObject;
            //valuesContainer = avatarStatusWindowMaker.transform.Find("Values").gameObject;
        }

        public void UpdateLayoutAndCamera()
        {
#if UNITY_EDITOR
            canvas.GetComponent<RectTransform>().sizeDelta = size;
            titleUI.text = displayName;
            var layout = statusLinesContainer.GetComponent<VerticalLayoutGroup>();
            if (layout == null) layout = statusLinesContainer.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.spacing = 0;
            layout.padding = new RectOffset(20, 20, 20, 0);
            var statusCount = statuses.Count;
            var statusUICount = statusLinesContainer.transform.childCount;
            //var valueUICount = valuesContainer.transform.childCount;
            for (var i = 0; i < statusCount; i++)
            {
                var status = statuses[i];
                var name = status.name;
                var min = status.min;
                var max = status.max;
                var value = status.value;
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
                if (valueUI.sharedMaterial != null && valueUI.sharedMaterial != Util.statusValueMaterial)
                {
                    Object.DestroyImmediate(valueUI.sharedMaterial);
                }
                valueUI.sharedMaterial = new Material(Util.statusValueMaterial);
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
                Object.DestroyImmediate(statusLine.transform.Find("Value/ValueModel").GetComponent<Renderer>().sharedMaterial);
                Object.DestroyImmediate(statusLine);
            }
            /*
            for (var i = statusCount; i < valueUICount; i++)
            {
                var valueObj = valuesContainer.transform.GetChild(i).gameObject;
                Object.DestroyImmediate(valueObj.GetComponent<Renderer>().sharedMaterial);
                Object.DestroyImmediate(valueObj);
            }
            */
            // Object.Object.DestroyImmediate(layout);
            SynchronizationContext.Current.Send(async state =>
            {
                await Task.Delay(100);
                Debug.Log("Object.DestroyImmediate");
                Debug.Log(statusLinesContainer);
                var layout = statusLinesContainer.GetComponent<VerticalLayoutGroup>();
                Debug.Log(layout);
                if (layout != null) Object.DestroyImmediate(layout);
            }, null);

            var renderTextureSize = AvatarStatusWindowMaker.RenderTextureSize(size);
            if (camera.targetTexture == null || camera.targetTexture.width != renderTextureSize.x || camera.targetTexture.height != renderTextureSize.y)
            {
                if (camera.targetTexture != null)
                {
                    var toDestroy = camera.targetTexture;
                    camera.targetTexture = null;
                    Object.DestroyImmediate(toDestroy);
                }
                camera.targetTexture = new RenderTexture(renderTextureSize.x, renderTextureSize.y, 24);
            }
            Debug.Log($"UpdateLayoutAndCamera end");
#endif
        }

        public void UpdateTexture()
        {
#if UNITY_EDITOR
            Debug.Log($"UpdateTexture start");
            var active = RenderTexture.active;
            // RenderTexture.active = camera.targetTexture;
            // camera.Render();
            var resizedRenderTexture = new RenderTexture(1024, 1024, 24);
            Debug.Log($"Graphics.Blit {camera.targetTexture} => {resizedRenderTexture}");
            Graphics.Blit(camera.targetTexture, resizedRenderTexture);
            RenderTexture.active = resizedRenderTexture;
            var texture = new Texture2D(resizedRenderTexture.width, resizedRenderTexture.height);
            texture.ReadPixels(new Rect(0, 0, resizedRenderTexture.width, resizedRenderTexture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = active;
            Object.DestroyImmediate(resizedRenderTexture);

            mesh.transform.localScale = new Vector3(size.x / 1000, size.y / 1000, 1);
            if (mesh.sharedMaterial != null)
            {
                if (mesh.sharedMaterial.mainTexture != null)
                {
                    Object.DestroyImmediate(mesh.sharedMaterial.mainTexture);
                }
                Object.DestroyImmediate(mesh.sharedMaterial);
            }
            mesh.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
            mesh.sharedMaterial.mainTexture = texture;
            Debug.Log($"UpdateTexture end");
#endif
        }
    }
}
