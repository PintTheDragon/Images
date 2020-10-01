using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

namespace Images
{
    public class Config : IConfig
    {
        [Description("Should the plugin be enabled or not.")]
        public bool IsEnabled { get; set; } = true;

        [Description("This is the list of every image. You need to add your image here before it can be used ingame.")]
        public List<ImageData> Images { get; set; } = new List<ImageData>(new ImageData[]
        {
            new ImageData("example1", "C:\\Temp\\example.png", false, "auto"), 
            new ImageData("example3", "https://pint.cloud/img2txt/smallimg.png", true, "26")
        });

        [Description("Should you need a specific permission to use a specific image. Permission: images.image.IMAGE_NAME")]
        public bool PerImagePermissions { get; set; } = false;
    }
}