using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using UnityEngine;

namespace Images.Commands
{
    public class IHint : ICommand
    {
        public string Command => "ihint";
        public string[] Aliases => new string[] {"imagehint"};
        public string Description => "Send an image through a hint.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender.CheckPermission("images.ihint") && arguments.Array != null && arguments.Array.Length > 1 && (arguments.Array[1].Trim().ToLower() == "reset" || arguments.Array[1].Trim().ToLower() == "none"))
            {
                foreach (var player in Player.List)
                {
                    player.ShowHint("", 1f);
                }
                
                response = "Reset hints.";
                return true;
            }
            
            HandleCommandObject obj = Util.HandleCommand(arguments, sender, out response, true, "ihint", "images.ihint");
            if (obj == null) return true;

            Timing.KillCoroutines(Images.Singleton.HintHandle);
            Images.Singleton.HintHandle = Timing.RunCoroutine(ShowHint(obj));
            Images.Singleton.Coroutines.Add(Images.Singleton.HintHandle);

            response = "Creating image and displaying hint.";
            return true;
        }

        private IEnumerator<float> ShowHint(HandleCommandObject obj)
        {
            List<string> frames = new List<string>();
            
            var startTime = DateTime.UtcNow;

            var handle = new CoroutineHandle();
            try
            {
                handle = Util.LocationToText(obj.image["location"], text =>
                    {
                        if (DateTime.UtcNow - startTime > TimeSpan.FromSeconds(obj.duration)) return;

                        var newText = text.Replace("\\n", "\n");

                        frames.Add(newText);

                        foreach (var player in Player.List)
                        {
                            player.ShowHint(newText, 2f);
                        }
                    }, obj.image["name"].Trim().ToLower(), obj.image["isURL"] == "true", obj.scale, false, .4f, obj.compress);
                Images.Singleton.Coroutines.Add(handle);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            
            yield return Timing.WaitUntilDone(handle);

            var cur = 0;
            
            if (frames.Count <= 1)
            {
                if (frames.Count == 1)
                {
                    foreach (var player in Player.List)
                    {
                        player.ShowHint(frames[0], obj.duration-2f);
                    }
                }
                yield break;
            }
            while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(obj.duration))
            {
                foreach (var player in Player.List)
                {
                    player.ShowHint(frames[cur % frames.Count], 2f);
                }

                yield return Timing.WaitForSeconds(.4f);

                cur++;
            }
        }
    }
}