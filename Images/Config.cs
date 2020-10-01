using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

namespace Images
{
    public class Config : IConfig
    {
        [Description("Should the plugin be enabled or not.")]
        public bool IsEnabled { get; set; } = true;

        [Description("This is the list of every image. You need to add your image here before it can be used ingame. Also, if you're using a url, make sure to set \"isURL\" to \"true\"!")]
        public List<Dictionary<string, string>> Images { get; set; } = new List<Dictionary<string, string>>(
        new []{
            new Dictionary<string, string>()
            {
                {"name", "example1"},
                {"location", "C:\\Temp\\example.png"},
                {"isURL", "false"},
                {"scale", "auto"}
            },
            new Dictionary<string, string>()
            {
                {"name", "example2"},
                {"location", "https://pint.cloud/img2txt/smallimg.png"},
                {"isURL", "true"},
                {"scale", "26"}
            }
        });

        [Description("Should you need a specific permission to use a specific image. Permission: images.image.IMAGE_NAME")]
        public bool PerImagePermissions { get; set; } = false;

        [Description("This will override the default intercom with a specific image. If you want this, put the image name below, otherwise, set it to \"none\".")]
        public string DefaultIntercomImage { get; set; } = "none";
    }
}