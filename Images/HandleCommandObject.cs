using System.Collections.Generic;

namespace Images
{
    public class HandleCommandObject
    {
        public Dictionary<string, string> image;
        public int duration;
        public float scale;

        public HandleCommandObject(Dictionary<string, string> image, int duration, float scale)
        {
            this.image = image;
            this.duration = duration;
            this.scale = scale;
        }
    }
}