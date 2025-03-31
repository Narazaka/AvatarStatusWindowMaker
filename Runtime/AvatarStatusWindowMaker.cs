using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Narazaka.VRChat.AvatarStatusWindowMaker.Editor")]

namespace Narazaka.VRChat.AvatarStatusWindowMaker
{
    [ExecuteInEditMode]
    public class AvatarStatusWindowMaker : MonoBehaviour, IEditorOnly
    {
        public static Vector2Int RenderTextureSize(Vector2 size) => new Vector2Int(Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y));
        public static float OrthographicSize(Vector2Int renderTextureSize, Vector3 lossyScale) => (float)renderTextureSize.y / 1000 / 2 * lossyScale.y;

        public string displayName = "あなたの名前";
        public Vector2 size = new Vector2(1000, 1000);
        public List<AvatarStatus> statuses = new ();
        public bool defaultActive = true;

        public Vector2Int renderTextureSize => RenderTextureSize(size);
        public float orthographicSize => OrthographicSize(renderTextureSize, transform.lossyScale);

        Camera _childCamera;
        Camera childCamera
        {
            get
            {
                if (_childCamera == null)
                {
                    var obj = transform.Find("Camera");
                    if (obj != null) _childCamera = obj.GetComponent<Camera>();
                }
                return _childCamera;
            }
        }

        UpdateElements _updateElements;
        UpdateElements updateElements
        {
            get
            {
                if (_updateElements == null) _updateElements = new UpdateElements(this);
                return _updateElements;
            }
        }

        bool needUpdateLayoutAndCamera;
        bool needUpdateTexture;

        void Update()
        {
            childCamera.orthographicSize = orthographicSize;
            if (needUpdateLayoutAndCamera)
            {
                updateElements.UpdateLayoutAndCamera();
                needUpdateLayoutAndCamera = false;
                needUpdateTexture = true;
            }
            else if (needUpdateTexture)
            {
                updateElements.UpdateTexture();
                needUpdateTexture = false;
            }
        }

        void OnValidate()
        {
            Render();
        }

        public void Render()
        {
            needUpdateLayoutAndCamera = true;
        }
    }
}
