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

            Timing.RunCoroutine(ShowHint(obj));

            response = "Creating image and displaying hint.";
            return true;
        }

        private IEnumerator<float> ShowHint(HandleCommandObject obj)
        {
            List<string> frames = new List<string>();
            
            yield return Timing.WaitUntilDone(Util.LocationToText(obj.image["location"], text =>
                {
                    frames.Add(text.Replace("\\n", "\n"));
                }, obj.image["name"].Trim().ToLower(), obj.image["isURL"] == "true", obj.scale, false)
            );

            var startTime = DateTime.UtcNow;

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