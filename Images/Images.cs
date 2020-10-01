using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Events.EventArgs;
using MEC;

namespace Images
{
    public class Images : Plugin<Config>
    {
        public override string Name { get; } = "Images";
        public override string Author { get; } = "PintTheDragon";
        public override Version Version { get; } = new Version(1, 0, 0);

        internal static Images Singleton;
        internal string IntercomText = null;

        public override void OnEnabled()
        {
            base.OnEnabled();

            Singleton = this;

            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            Exiled.Events.Handlers.Player.Joined += OnPlayerJoin;
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            Singleton = null;
            
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Exiled.Events.Handlers.Player.Joined -= OnPlayerJoin;
        }

        private void OnPlayerJoin(JoinedEventArgs ev)
        {
            if(IntercomText != null) Timing.CallDelayed(2f, () => ev.Player.ReferenceHub.GetComponent<Intercom>().CustomContent = IntercomText);
        }

        private void OnRoundStart()
        {
            IntercomText = null;
            
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

            try
            {
                IntercomText = API.LocationToText(image["location"], image["isURL"] == "true", scale).Replace("\\n", "\n");
                ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = IntercomText;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}