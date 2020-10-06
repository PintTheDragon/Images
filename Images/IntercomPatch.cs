using Exiled.API.Features;
using HarmonyLib;
using MEC;
using UnityEngine;

namespace Images
{
    [HarmonyPatch(typeof(Intercom), nameof(Intercom.UpdateText))]
    internal static class IntercomPatch
    {
        private static bool Prefix(Intercom __instance)
        {
            if (__instance.remainingCooldown > 0f)
            {
                if (Images.Singleton.ICool) return true;
                
                Images.Singleton.IntercomText = null;
                ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = "";
                    
                Images.Singleton.ICool = true;

                Images.Singleton.IReady = false;
                Images.Singleton.ITrans = false;

                Timing.KillCoroutines(Images.Singleton.IntercomHandle);

                Images.Singleton.RunIntercomImage(Images.Singleton.Config.DefaultIntercomImageCooldown);
            }
            else if (__instance.Networkspeaker != null)
            {
                if (Images.Singleton.ITrans) return true;
                
                Images.Singleton.IntercomText = null;
                ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = "";
                    
                Images.Singleton.ITrans = true;

                Images.Singleton.IReady = false;
                Images.Singleton.ICool = false;

                Timing.KillCoroutines(Images.Singleton.IntercomHandle);

                Images.Singleton.RunIntercomImage(Images.Singleton.Config.DefaultIntercomImageSpeaking);
            }
            else
            {
                if (Images.Singleton.IReady) return true;
                
                Images.Singleton.IntercomText = null;
                ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = "";
                    
                Images.Singleton.IReady = true;

                Images.Singleton.ITrans = false;
                Images.Singleton.ICool = false;
                    
                Timing.KillCoroutines(Images.Singleton.IntercomHandle);
                
                Images.Singleton.RunIntercomImage(Images.Singleton.Config.DefaultIntercomImageReady);
            }
            
            return true;
        }
    }
}