using System.Collections.Generic;

namespace Images
{
    public class HandleCommandObject
    {
        public Dictionary<string, string> image;
        public int duration;
        public float scale;
        public float fps;

        public HandleCommandObject(Dictionary<string, string> image, int duration, float scale, float fps)
        {
            this.image = image;
            this.duration = duration;
            this.scale = scale;
            this.fps = fps;
        }
    }
}