using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using RemoteAdmin;

namespace Images.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class IBroadcast : ICommand
    {
        public string Command => "ibroadcast";
        public string[] Aliases => new string[] {"ibc"};
        public string Description => "Send an image through a broadcast.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            HandleCommandObject obj = Util.HandleCommand(arguments, sender, out response, true, "ibroadcast", "images.ibc");
            if (obj == null) return true;

            var text = API.LocationToText(obj.image[1], obj.image[2] == "true", obj.scale);
            
            foreach (var player in Player.List)
            {
                player.Broadcast((ushort)obj.duration, text);
            }

            response = "Successfully broadcast image.";
            return true;
        }
    }
}