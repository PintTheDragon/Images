using System;
using System.Collections.Generic;
using CommandSystem;
using Exiled.API.Features;
using MEC;
using UnityEngine;

namespace Images.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class IHint : ICommand
    {
        public string Command => "ihint";
        public string[] Aliases => new string[] {"imagehint"};
        public string Description => "Send an image through a hint.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            HandleCommandObject obj = Util.HandleCommand(arguments, sender, out response, true, "ihint", "images.ihint");
            if (obj == null) return true;

            Images.Singleton.Coroutines.Add(Timing.RunCoroutine(ShowHint(obj)));

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
                        var newText = text.Replace("\\n", "\n");

                        frames.Add(newText);

                        foreach (var player in Player.List)
                        {
                            player.ShowHint(newText, 2f);
                        }
                    }, obj.image["name"].Trim().ToLower(), obj.image["isURL"] == "true", obj.scale, false, .4f);
                Images.Singleton.Coroutines.Add(handle);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            
            yield return Timing.WaitUntilDone(handle);

            var cur = 0;
                
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