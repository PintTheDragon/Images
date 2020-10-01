using System;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Interfaces;

namespace Images
{
    public class Images : Plugin<Config>
    {
        public override string Name { get; } = "Images";
        public override string Author { get; } = "PintTheDragon";
        public override Version Version { get; } = new Version(1, 0, 0);

        public static Images Singleton;

        public override void OnEnabled()
        {
            base.OnEnabled();

            Singleton = this;

            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            Singleton = null;
            
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
        }

        private void OnRoundStart()
        {
            var imageName = Config.DefaultIntercomImage.Trim().ToLower().Replace(" ", "");
            
            if (imageName != "none" && Config.Images.Count(img => img[0].Trim().ToLower().Replace(" ", "") == imageName) > 0)
            {
                var image = Config.Images.First(img => img[0].Trim().ToLower().Replace(" ", "") == imageName);
                
                var scale = 0;

                if (image[3].Trim().ToLower() != "auto")
                {
                    if (!int.TryParse(image[3].Trim().ToLower(), out scale))
                    {
                        Log.Error("The scale value for the custom intercom image is incorrect. Use an integer or \"auto\".");
                        return;
                    }
                }
                
                ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = API.LocationToText(image[1], image[2] == "true", scale).Replace("\\n", "\n");
            }
        }
    }
}