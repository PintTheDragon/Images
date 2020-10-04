using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
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
        internal Dictionary<string, List<string>> ImageCache = new Dictionary<string, List<string>>();
        internal CoroutineHandle IntercomHandle;
        internal List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();

        public override void OnEnabled()
        {
            base.OnEnabled();

            Singleton = this;

            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            Exiled.Events.Handlers.Server.RestartingRound += OnRoundRestart;
            Exiled.Events.Handlers.Player.Joined += OnPlayerJoin;
            Exiled.Events.Handlers.Server.ReloadedConfigs += OnConfigReloaded;
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            Timing.KillCoroutines(Coroutines);
            Coroutines.Clear();

            Singleton = null;
            
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Exiled.Events.Handlers.Server.RestartingRound -= OnRoundRestart;
            Exiled.Events.Handlers.Player.Joined -= OnPlayerJoin;
            Exiled.Events.Handlers.Server.ReloadedConfigs -= OnConfigReloaded;
            
            ImageCache.Clear();
        }

        private void OnPlayerJoin(JoinedEventArgs ev)
        {
            if(IntercomText != null) Timing.CallDelayed(2f, () => ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = IntercomText);
        }

        private void OnConfigReloaded()
        {
            Timing.KillCoroutines(Coroutines);
            Coroutines.Clear();
            
            ImageCache.Clear();
            
            OnRoundStart();
        }

        private void OnRoundRestart()
        {
            Timing.KillCoroutines(Coroutines);
            Coroutines.Clear();
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

                if (IntercomHandle.IsRunning) Timing.KillCoroutines(IntercomHandle);
                IntercomHandle = Timing.RunCoroutine(ShowIntercom(image, scale));
                Coroutines.Add(IntercomHandle);
            }
        }

        private IEnumerator<float> ShowIntercom(Dictionary<string, string> image, int scale)
        {
            List<string> frames = new List<string>();

            CoroutineHandle handle = new CoroutineHandle();
            try
            {
                handle = Util.LocationToText(image["location"], text =>
                    {
                        IntercomText = text.Replace("\\n", "\n");
                        ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = IntercomText;
                        frames.Add(IntercomText);
                    }, image["name"].Trim().ToLower(), image["isURL"] == "true", scale);
                
                Coroutines.Add(handle);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            yield return Timing.WaitUntilDone(handle);

            var cur = 0;
            
            while (true)
            {
                IntercomText = frames[cur % frames.Count];
                ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = IntercomText;

                yield return Timing.WaitForSeconds(.1f);

                cur++;
            }
        }
    }
}