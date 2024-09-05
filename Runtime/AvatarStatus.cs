using System;

namespace Narazaka.VRChat.AvatarStatusWindowMaker
{
    [Serializable]
    public class AvatarStatus
    {
        public static float ValueRate(float min, float max, float value)
        {
            return (value - min) / (max - min);
        }
        public string name = "";
        public float min = 0;
        public float max = 100;
        public float value = 50;
        public bool menu;
        public bool saved;
        public float valueRate => ValueRate(min, max, value);
    }
}
