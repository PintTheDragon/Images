namespace Images
{
    public class FrameData
    {
        public string Data;
        public bool Last = false;

        public FrameData(string data)
        {
            Data = data;
        }
    }
}