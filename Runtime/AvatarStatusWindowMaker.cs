using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

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

        void Update()
        {
            childCamera.orthographicSize = orthographicSize;
        }
    }
}
