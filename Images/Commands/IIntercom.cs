using System;
using CommandSystem;

namespace Images.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class IIntercom : ICommand
    {
        public string Command => "iintercom";
        public string[] Aliases => null;
        public string Description => "Set the intercom text to an image.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            HandleCommandObject obj = Util.HandleCommand(arguments, sender, out response, false, "iintercom", "images.iintercom");
            if (obj == null) return true;

            var text = API.LocationToText(obj.image["location"], obj.image["isURL"] == "true", obj.scale).Replace("\\n", "\n");

            ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = text;

            response = "Successfully set intercom text.";
            return true;
        }
    }
}