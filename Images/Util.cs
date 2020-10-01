using System;
using System.Linq;
using CommandSystem;
using Exiled.Permissions.Extensions;
using RemoteAdmin;

namespace Images
{
    public class Util
    {
        public static HandleCommandObject HandleCommand(ArraySegment<string> arguments, ICommandSender sender, out string response, bool doDuration, string name, string perm)
        {
            var permission = false;
            var perPermission = false;

            if (arguments.Array == null || arguments.Array.Length < 3)
            {
                response = "Usage: "+name+(doDuration ? " <time>" : "")+" <image name>";
                return null;
            }

            var imageName = arguments.Array[2].Trim().ToLower().Replace(" ", "");
            
            var duration = 0;
            if (doDuration)
            {
                if (!int.TryParse(arguments.Array[1].Trim().ToLower(), out duration))
                {
                    response = "Usage: "+name+" <time> <image name>";
                    return null;
                }
            }

            if (sender is PlayerCommandSender p)
            {
                permission = p.CheckPermission(perm);
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
                return null;
            }
            
            var imageList = Images.Singleton.Config.Images.Where(img => imageName == img[0].Trim().ToLower().Replace(" ", "")).ToArray();
            if (imageList.Length < 1)
            {
                response = "No images for this name were found. Add the image to your config first.";
                return null;
            }

            var image = imageList[0];

            var scale = 0;

            if (image[3].Trim().ToLower() != "auto")
            {
                if (!int.TryParse(image[3].Trim().ToLower(), out scale))
                {
                    response = "The scale parameter for this image is invalid. Only use integers or \"auto\".";
                    return null;
                }
            }

            response = "Error";

            return new HandleCommandObject(imageList[0], duration, scale);
        }
    }
}