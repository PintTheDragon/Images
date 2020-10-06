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
            if (!string.IsNullOrEmpty(__instance.CustomContent))
            {
                __instance._content = __instance.CustomContent;
            }
            else if (__instance.Muted)
            {
                __instance._content = "YOU ARE MUTED BY ADMIN";
                
                Images.Singleton.ICool = false;
                Images.Singleton.IReady = false;
                Images.Singleton.ITrans = false;
            }
            else if (Intercom.AdminSpeaking)
            {
                __instance._content = "ADMIN IS USING\nTHE INTERCOM NOW";
                
                Images.Singleton.ICool = false;
                Images.Singleton.IReady = false;
                Images.Singleton.ITrans = false;
            }
            else if (__instance.remainingCooldown > 0f)
            {
                __instance._content = "RESTARTING\n" + Mathf.CeilToInt(__instance.remainingCooldown);

                if (!Images.Singleton.ICool)
                {
                    Images.Singleton.ICool = true;

                    Images.Singleton.IReady = false;
                    Images.Singleton.ITrans = false;
                    
                    Timing.KillCoroutines(Images.Singleton.IntercomHandle);
                
                    Images.Singleton.RunIntercomImage(Images.Singleton.Config.DefaultIntercomImageCooldown);
                }
            }
            else if (__instance.Networkspeaker != null)
            {
                if (__instance.speechRemainingTime == -77f)
                {
                    __instance._content = "TRANSMITTING...\nBYPASS MODE";
                    
                    if (!Images.Singleton.ITrans)
                    {
                        Images.Singleton.ITrans = true;

                        Images.Singleton.IReady = false;
                        Images.Singleton.ICool = false;
                        
                        Timing.KillCoroutines(Images.Singleton.IntercomHandle);
                
                        Images.Singleton.RunIntercomImage(Images.Singleton.Config.DefaultIntercomImageSpeaking);
                    }
                }
                else
                {
                    __instance._content = "TRANSMITTING...\nTIME LEFT - " + Mathf.CeilToInt(__instance.speechRemainingTime);
                    
                    if (!Images.Singleton.ITrans)
                    {
                        Images.Singleton.ITrans = true;

                        Images.Singleton.IReady = false;
                        Images.Singleton.ICool = false;
                        
                        Timing.KillCoroutines(Images.Singleton.IntercomHandle);
                        
                        Images.Singleton.RunIntercomImage(Images.Singleton.Config.DefaultIntercomImageSpeaking);
                    }
                }
            }
            else
            {
                __instance._content = "READY";
                
                if (!Images.Singleton.IReady)
                {
                    Images.Singleton.IReady = true;

                    Images.Singleton.ITrans = false;
                    Images.Singleton.ICool = false;
                    
                    Timing.KillCoroutines(Images.Singleton.IntercomHandle);
                
                    Images.Singleton.RunIntercomImage(Images.Singleton.Config.DefaultIntercomImageReady);
                }
            }
            if (__instance._contentDirty)
            {
                __instance.NetworkintercomText = __instance._content;
                __instance._contentDirty = false;
            }
            if (Intercom.AdminSpeaking == Intercom.LastState)
            {
                return false;
            }
            Intercom.LastState = Intercom.AdminSpeaking;
            __instance.RpcUpdateAdminStatus(Intercom.AdminSpeaking);

            return false;
        }
    }
}