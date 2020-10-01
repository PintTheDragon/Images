namespace Images
{
    public class ImageData
    {
        public string Name;
        public string Location;
        public bool isURL;
        public string scale;

        public ImageData(string name, string location, bool isUrl, string scale)
        {
            Name = name;
            Location = location;
            isURL = isUrl;
            this.scale = scale;
        }
    }
}