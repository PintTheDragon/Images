using System;
using CommandSystem;
using Exiled.API.Features;

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

            var text = API.LocationToText(obj.image["location"], obj.image["isURL"] == "true", obj.scale);
            
            foreach (var player in Player.List)
            {
                player.Broadcast((ushort)obj.duration, text);
            }

            response = "Successfully broadcast image.";
            return true;
        }
    }
}