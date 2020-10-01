namespace Images
{
    public class HandleCommandObject
    {
        public string[] image;
        public int duration;
        public float scale;

        public HandleCommandObject(string[] image, int duration, float scale)
        {
            this.image = image;
            this.duration = duration;
            this.scale = scale;
        }
    }
}