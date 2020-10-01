using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using MEC;

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
            
            if (imageName != "none" && Config.Images.Count(img => img["name"].Trim().ToLower().Replace(" ", "") == imageName) > 0)
            {
                var image = Config.Images.First(img => img["name"].Trim().ToLower().Replace(" ", "") == imageName);
                
                var scale = 0;

                if (image["scale"].Trim().ToLower() != "auto")
                {
                    if (!int.TryParse(image["scale"].Trim().ToLower(), out scale))
                    {
                        Log.Error("The scale value for the custom intercom image is incorrect. Use an integer or \"auto\".");
                        return;
                    }
                }

                Timing.RunCoroutine(Util.TimeoutCoroutine(Timing.RunCoroutine(ShowIntercom(image, scale))));
            }
        }

        private IEnumerator<float> ShowIntercom(Dictionary<string, string> image, int scale)
        {
            yield return Timing.WaitForSeconds(0.1f);
            
            ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = API.LocationToText(image["location"], image["isURL"] == "true", scale).Replace("\\n", "\n");
        }
    }
}