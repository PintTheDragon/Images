using System;

namespace Images
{
    public class FrameData
    {
        public string Data;
        public bool Last = false;
        public Exception Error = null;

        public FrameData(string data)
        {
            Data = data;
        }
    }
}