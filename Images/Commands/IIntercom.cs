using System;
using System.Collections.Generic;
using CommandSystem;
using Exiled.API.Features;
using MEC;

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

            Timing.RunCoroutine(Util.TimeoutCoroutine(Timing.RunCoroutine(ShowIntercom(obj))));

            response = "Successfully set intercom text.";
            return true;
        }

        private IEnumerator<float> ShowIntercom(HandleCommandObject obj)
        {
            yield return Timing.WaitForSeconds(0.1f);

            try
            {
                Images.Singleton.IntercomText = Util.LocationToText(obj.image["location"], obj.image["name"].Trim().ToLower(), obj.image["isURL"] == "true", obj.scale)
                    .Replace("\\n", "\n");

                ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = Images.Singleton.IntercomText;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}