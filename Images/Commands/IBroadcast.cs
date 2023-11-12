using System;
using System.Collections.Generic;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
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
            if (sender.CheckPermission("images.ibc") && arguments.Array != null && arguments.Array.Length > 1 && (arguments.Array[1].Trim().ToLower() == "reset" || arguments.Array[1].Trim().ToLower() == "none"))
            {
                foreach (var player in Player.List)
                {
                    player.ClearBroadcasts();
                }
                
                response = "Reset broadcasts.";
                return true;
            }
            
            HandleCommandObject obj = Util.HandleCommand(arguments, sender, out response, true, "ibroadcast", "images.ibc");
            if (obj == null) return true;

            Timing.KillCoroutines(Images.Singleton.BroadcastHandle);
            Images.Singleton.BroadcastHandle = Timing.RunCoroutine(ShowBroadcast(obj));
            Images.Singleton.Coroutines.Add(Images.Singleton.BroadcastHandle);

            response = "Creating image and displaying broadcast.";
            return true;
        }

        private IEnumerator<float> ShowBroadcast(HandleCommandObject obj)
        {
            yield return Timing.WaitForSeconds(0.1f);

            try
            {
                var handle = Util.LocationToText(obj.image["location"], text =>
                {
                    foreach (var player in Player.List)
                    {
                        player.Broadcast((ushort) obj.duration, text);
                    }
                }, obj.image["name"].Trim().ToLower(), obj.image["isURL"] == "true", obj.scale, true, obj.fps, obj.compress);
                Images.Singleton.Coroutines.Add(handle);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}