using System;
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
        public string Description => "This is the command used to run custom events with EasyEvents.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var permission = false;
            var perPermission = false;

            if (arguments.Array == null || arguments.Array.Length < 3)
            {
                response = "Usage: ibroadcast <time> <image name>";
                return true;
            }

            var imageName = arguments.Array[1].Trim().ToLower().Replace(" ", "");
            if (!int.TryParse(arguments.Array[2].Trim(), out var duration))
            {
                response = "Usage: ibroadcast <time> <image name>";
                return true;
            }

            if (sender is PlayerCommandSender p)
            {
                permission = p.CheckPermission("images.ibc");
                perPermission = p.CheckPermission("images.image." + imageName);
            }
            else
            {
                permission = true;
                perPermission = true;
            }

            if (!permission || (!perPermission && Images.Singleton.Config.PerImagePermissions))
            {
                response = "Permission denied.";
                return true;
            }
            
            var imageList = Images.Singleton.Config.Images.Where(img => imageName == img.Name.Trim().ToLower().Replace(" ", "")).ToArray();
            if (imageList.Length < 1)
            {
                response = "No images for this name were found. Add the image to your config first.";
                return true;
            }

            var image = imageList[0];
            
            foreach (var player in Player.List)
            {
                player.Broadcast((ushort)duration, API.LocationToText(image.Location, image.isURL));
            }

            response = "Successfully broadcast image.";
            return true;
        }
    }
}