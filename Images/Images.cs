using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using HarmonyLib;
using Images.Commands;
using MEC;
using RemoteAdmin;

namespace Images
{
    public class Images : Plugin<Config>
    {
        public override string Name { get; } = "Images";
        public override string Author { get; } = "PintTheDragon";
        public override Version Version { get; } = new Version(1, 1, 1);

        internal static Images Singleton;
        internal string IntercomText = null;
        internal Dictionary<string, List<string>> ImageCache = new Dictionary<string, List<string>>();
        internal CoroutineHandle IntercomHandle;
        internal List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();
        internal bool CacheReady = true;

        internal bool IReady = true;
        internal bool ITrans = false;
        internal bool ICool = false;

        private Harmony harmony;
        private List<ICommand> commands = new List<ICommand>();
        private CoroutineHandle preCache;

        public override void OnEnabled()
        {
            base.OnEnabled();
            
            if(Config.EnablePrecache) CacheReady = false;

            Singleton = this;
            
            harmony = new Harmony("PintImages");
            harmony.PatchAll();

            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            Exiled.Events.Handlers.Server.RestartingRound += OnRoundRestart;
            Exiled.Events.Handlers.Player.Joined += OnPlayerJoin;
            Exiled.Events.Handlers.Server.ReloadedConfigs += OnConfigReloaded;
            
            commands.Clear();
            commands.Add(new IBroadcast());
            commands.Add(new IHint());
            commands.Add(new IIntercom());
            
            foreach (var command in commands)
            {
                CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(command);
                GameCore.Console.singleton.ConsoleCommandHandler.RegisterCommand(command);
            }

            preCache = Timing.RunCoroutine(RunPreCache());
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            
            if(Config.EnablePrecache) CacheReady = false;

            Timing.KillCoroutines(IntercomHandle);
            IntercomHandle = new CoroutineHandle();
            
            Timing.KillCoroutines(Coroutines);
            Coroutines.Clear();

            Singleton = null;
            
            harmony.UnpatchAll();
            
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Exiled.Events.Handlers.Server.RestartingRound -= OnRoundRestart;
            Exiled.Events.Handlers.Player.Joined -= OnPlayerJoin;
            Exiled.Events.Handlers.Server.ReloadedConfigs -= OnConfigReloaded;
            
            foreach (var command in commands)
            {
                CommandProcessor.RemoteAdminCommandHandler.UnregisterCommand(command);
                GameCore.Console.singleton.ConsoleCommandHandler.UnregisterCommand(command);
            }

            commands.Clear();
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
            if(Config.EnablePrecache) CacheReady = false;
            
            Timing.KillCoroutines(IntercomHandle);
            IntercomHandle = new CoroutineHandle();
            
            Timing.KillCoroutines(Coroutines);
            Coroutines.Clear();
            
            ImageCache.Clear();
            preCache = Timing.RunCoroutine(RunPreCache());
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

        private IEnumerator<float> RunPreCache()
        {
            if (!Config.EnablePrecache) yield break;
            
            foreach (var image in Config.Images)
            {
                if (image.ContainsKey("precache") && image["precache"].Trim().ToLower() == "true")
                {
                    var scale = 0;

                    if (image.ContainsKey("scale") && image["scale"].Trim().ToLower() != "auto" && !int.TryParse(image["scale"].Trim().ToLower(), out scale))
                    {
                        Log.Error("The scale value for the custom intercom image is incorrect. Use an integer or \"auto\".");
                        continue;
                    }

                    var handle = Util.LocationToText(image["location"], text => {}, image["name"].Trim().ToLower(), image["isURL"] == "true", scale, true, 0f);
                    Coroutines.Add(handle);
                    yield return Timing.WaitUntilDone(handle);

                    yield return Timing.WaitForSeconds(10f);
                    
                    handle = Util.LocationToText(image["location"], text => {}, image["name"].Trim().ToLower(), image["isURL"] == "true", scale, false, 0f);
                    Coroutines.Add(handle);
                    yield return Timing.WaitUntilDone(handle);
                }
            }

            CacheReady = true;
            
            Log.Info("Finished pre-caching items!");
        }

        private void OnRoundStart()
        {
            RunIntercomImage(Config.DefaultIntercomImage);
        }

        internal void RunIntercomImage(string imageName)
        {
            if (imageName == "none" || Config.Images.Count(img => img["name"].Trim().ToLower().Replace(" ", "") == imageName) <= 0) return;
            
            var image = Config.Images.First(img => img["name"].Trim().ToLower().Replace(" ", "") == imageName);
                
            var scale = 0;

            if (image.ContainsKey("scale") && image["scale"].Trim().ToLower() != "auto" && !int.TryParse(image["scale"].Trim().ToLower(), out scale))
            {
                Log.Error("The scale value for the custom intercom image is incorrect. Use an integer or \"auto\".");
                return;
            }
                
            var fps = 10;

            if (image.ContainsKey("fps") && image["fps"].Trim().ToLower() != "auto" && !int.TryParse(image["fps"].Trim().ToLower(), out fps))
            {
                Log.Error("The fps value for the custom intercom image is incorrect. Use an integer.");
                return;
            }

            Timing.KillCoroutines(IntercomHandle);
                
            IntercomText = null;
            ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = "";
                
            IntercomHandle = Timing.RunCoroutine(ShowIntercom(image, scale, fps));
            Coroutines.Add(IntercomHandle);
        }

        private IEnumerator<float> ShowIntercom(Dictionary<string, string> image, int scale, float fps)
        {
            yield return Timing.WaitUntilDone(preCache);
            
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