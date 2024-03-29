﻿using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

namespace Images
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        
        public bool Debug { get; set; } = false;

        [Description("This is the list of every image. You need to add your image here before it can be used ingame. Also, if you're using a url, make sure to set \"isURL\" to \"true\"!")]
        public List<Dictionary<string, string>> Images { get; set; } = new List<Dictionary<string, string>>(
        new []{
            new Dictionary<string, string>()
            {
                {"name", "example1"},
                {"location", "C:\\Temp\\example.png"},
                {"isURL", "false"},
                {"scale", "auto"},
                {"fps", "auto"}
            },
            new Dictionary<string, string>()
            {
                {"name", "example2"},
                {"location", "https://pint.cloud/img2txt/smallimg.png"},
                {"isURL", "true"},
                {"scale", "26"},
                {"fps", "auto"}
            }
        });

        [Description("Should you need a specific permission to use a specific image. Permission: images.image.IMAGE_NAME")]
        public bool PerImagePermissions { get; set; } = false;

        [Description("How many images should be cached. Caching helps reduce load on the server but will increase RAM usage. If too much RAM is being used, lower this, and if sending images takes too much time, increase this. Set it to 0 to disable caching.")]
        public int CacheSize { get; set; } = 20;

        [Description("Enable precaching. This will cause performence issues when the server starts up and during reloads.")]
        public bool EnablePrecache { get; set; } = false;
    }
}