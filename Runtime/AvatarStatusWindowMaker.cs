using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace Narazaka.VRChat.AvatarStatusWindowMaker
{
    public class AvatarStatusWindowMaker : MonoBehaviour, IEditorOnly
    {
        public string displayName = "あなたの名前";
        public Vector2 size = new Vector2(1000, 1000);
        public List<AvatarStatus> statuses = new ();
    }
}
