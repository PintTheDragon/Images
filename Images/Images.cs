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
            Exiled.Events.Handlers.Player.IntercomSpeaking += OnIntercomTalk;

            ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = "";
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            Timing.KillCoroutines(IntercomHandle);
            IntercomHandle = new CoroutineHandle();
            
            Timing.KillCoroutines(Coroutines);
            Coroutines.Clear();

            Singleton = null;
            
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Exiled.Events.Handlers.Server.RestartingRound -= OnRoundRestart;
            Exiled.Events.Handlers.Player.Joined -= OnPlayerJoin;
            Exiled.Events.Handlers.Server.ReloadedConfigs -= OnConfigReloaded;
            Exiled.Events.Handlers.Player.IntercomSpeaking -= OnIntercomTalk;
            
            ImageCache.Clear();
            
            ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = "";
            IntercomText = null;
        }

        private void OnPlayerJoin(JoinedEventArgs ev)
        {
            if(IntercomText != null) Timing.CallDelayed(2f, () => ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = IntercomText);
        }

        private void OnConfigReloaded()
        {
            Timing.KillCoroutines(IntercomHandle);
            IntercomHandle = new CoroutineHandle();
            
            Timing.KillCoroutines(Coroutines);
            Coroutines.Clear();
            
            ImageCache.Clear();
            
            OnRoundStart();
            
            ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = "";
            IntercomText = null;
        }

        private void OnRoundRestart()
        {
            Timing.KillCoroutines(IntercomHandle);
            IntercomHandle = new CoroutineHandle();
            
            Timing.KillCoroutines(Coroutines);
            Coroutines.Clear();
            ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = "";
        }

        private void OnIntercomTalk(IntercomSpeakingEventArgs ev)
        {
            if (ev.Player == null)
            {
                if (Config.DefaultIntercomImageSpeaking != "none")
                {
                    Timing.KillCoroutines(IntercomHandle);
                    IntercomHandle = new CoroutineHandle();
                    
                    ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = "";
                    IntercomText = null;
                }
            }
            else
            {
                RunIntercomImage(Config.DefaultIntercomImageSpeaking);
            }
        }

        private void OnRoundStart()
        {
            IntercomText = null;
            ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = "";

            RunIntercomImage(Config.DefaultIntercomImage);
        }

        private void RunIntercomImage(string imageName)
        {
            if (imageName != "none" && Config.Images.Count(img => img["name"].Trim().ToLower().Replace(" ", "") == imageName) > 0)
            {
                var image = Config.Images.First(img => img["name"].Trim().ToLower().Replace(" ", "") == imageName);
                
                var scale = 0;

                if (image.ContainsKey("scale") && image["scale"].Trim().ToLower() != "auto")
                {
                    if (!int.TryParse(image["scale"].Trim().ToLower(), out scale))
                    {
                        Log.Error("The scale value for the custom intercom image is incorrect. Use an integer or \"auto\".");
                        return;
                    }
                }
                
                var fps = 10;

                if (image.ContainsKey("fps") && image["fps"].Trim().ToLower() != "auto")
                {
                    if (!int.TryParse(image["fps"].Trim().ToLower(), out fps))
                    {
                        Log.Error("The fps value for the custom intercom image is incorrect. Use an integer.");
                        return;
                    }
                }

                if (IntercomHandle.IsRunning) Timing.KillCoroutines(IntercomHandle);
                IntercomHandle = Timing.RunCoroutine(ShowIntercom(image, scale, fps));
                Coroutines.Add(IntercomHandle);
            }
        }

        private IEnumerator<float> ShowIntercom(Dictionary<string, string> image, int scale, float fps)
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
                    }, image["name"].Trim().ToLower(), image["isURL"] == "true", scale, true, 1/fps);
                
                Coroutines.Add(handle);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            yield return Timing.WaitUntilDone(handle);

            var cur = 0;

            if (frames.Count > 1)
            {
                while (true)
                {
                    IntercomText = frames[cur % frames.Count];
                    ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = IntercomText;

                    yield return Timing.WaitForSeconds(1/fps);

                    cur++;
                }
            }
        }
    }
}