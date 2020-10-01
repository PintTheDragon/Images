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
        public List<string[]> Images { get; set; } = new List<string[]>(
        new []{
            new [] {"example1", "C:\\Temp\\example.png", "false", "auto"},
            new [] {"example3", "https://pint.cloud/img2txt/smallimg.png", "true", "26"}
        });

        [Description("Should you need a specific permission to use a specific image. Permission: images.image.IMAGE_NAME")]
        public bool PerImagePermissions { get; set; } = false;
    }
}