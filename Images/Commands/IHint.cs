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

            Timing.RunCoroutine(Util.TimeoutCoroutine(Timing.RunCoroutine(ShowHint(obj))));

            response = "Creating image and displaying hint.";
            return true;
        }

        private IEnumerator<float> ShowHint(HandleCommandObject obj)
        {
            yield return Timing.WaitForSeconds(0.1f);

            try
            {
                var text = API.LocationToText(obj.image["location"], obj.image["isURL"] == "true", obj.scale)
                    .Replace("\\n", "\n");

                foreach (var player in Player.List)
                {
                    player.ShowHint(text, obj.duration);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}