using System;
using CommandSystem;
using Exiled.API.Features;

namespace Images.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class IHint : ICommand
    {
        public string Command => "ihint";
        public string[] Aliases => null;
        public string Description => "Send an image through a hint.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            HandleCommandObject obj = Util.HandleCommand(arguments, sender, out response, true, "ihint", "images.ihint");
            if (obj == null) return true;

            var text = API.LocationToText(obj.image[1], obj.image[2] == "true", obj.scale);
            
            foreach (var player in Player.List)
            {
                player.ShowHint(text, obj.duration);
            }

            response = "Successfully displayed hint.";
            return true;
        }
    }
}