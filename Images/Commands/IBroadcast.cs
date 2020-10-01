using System;
using System.Collections.Generic;
using CommandSystem;
using Exiled.API.Features;
using MEC;

namespace Images.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class IBroadcast : ICommand
    {
        public string Command => "ibroadcast";
        public string[] Aliases => new string[] {"ibc", "imagebc", "imagebroadcast"};
        public string Description => "Send an image through a broadcast.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            HandleCommandObject obj = Util.HandleCommand(arguments, sender, out response, true, "ibroadcast", "images.ibc");
            if (obj == null) return true;

            Timing.RunCoroutine(Util.TimeoutCoroutine(Timing.RunCoroutine(ShowBroadcast(obj))));

            response = "Creating image and displaying broadcast.";
            return true;
        }

        private IEnumerator<float> ShowBroadcast(HandleCommandObject obj)
        {
            yield return Timing.WaitForSeconds(0.1f);

            try
            {
                var text = Util.LocationToText(obj.image["location"], obj.image["name"].Trim().ToLower(), obj.image["isURL"] == "true", obj.scale);

                foreach (var player in Player.List)
                {
                    player.Broadcast((ushort) obj.duration, text);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}