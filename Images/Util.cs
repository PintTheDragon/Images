﻿using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using RemoteAdmin;

namespace Images
{
    internal static class Util
    {
        static CoroutineHandle ActiveJob;
        internal static HandleCommandObject HandleCommand(ArraySegment<string> arguments, ICommandSender sender, out string response, bool doDuration, string name, string perm)
        {
            var permission = false;
            var perPermission = false;

            if (arguments.Array == null || arguments.Array.Length < (doDuration ? 3 : 2))
            {
                response = "Usage: "+name+(doDuration ? " <time>" : "")+" <image name>";
                return null;
            }

            var imageName = arguments.Array[(doDuration ? 2 : 1)].Trim().ToLower().Replace(" ", "");
            
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
            
            var imageList = Images.Singleton.Config.Images.Where(img => imageName == img["name"].Trim().ToLower().Replace(" ", "")).ToArray();
            if (imageList.Length < 1)
            {
                response = "No images for this name were found. Add the image to your config first.";
                return null;
            }

            var image = imageList[0];

            var scale = 0;

            if (image["scale"].Trim().ToLower() != "auto")
            {
                if (!int.TryParse(image["scale"].Trim().ToLower(), out scale))
                {
                    response = "The scale parameter for this image is invalid. Only use integers or \"auto\".";
                    return null;
                }
            }

            response = "Error";

            return new HandleCommandObject(imageList[0], duration, scale);
        }

        internal static IEnumerator<float> TimeoutCoroutine(CoroutineHandle coroutine)
        {
            yield return Timing.WaitForSeconds(5f);

            if (coroutine.IsRunning)
            {
                Log.Error("Creating an image took too long. Stopping execution.");
                Timing.KillCoroutines(coroutine);
            }
        }

        internal static void LocationToText(string loc, Action<string> handle, string name, bool isURL = false, float scale = 0f, bool shapeCorrection = true)
        {
            if (Images.Singleton.ImageCache.Count > Images.Singleton.Config.CacheSize)
            {
                Images.Singleton.ImageCache.Remove(Images.Singleton.ImageCache.Keys.PickRandom());
            }
            
            if (!Images.Singleton.ImageCache.ContainsKey(name))
            {
                if (ActiveJob.IsRunning) Timing.KillCoroutines(ActiveJob);
                Images.Singleton.ImageCache[name] = new List<string>();
                ActiveJob = API.LocationToText(loc, data =>
                {
                    Images.Singleton.ImageCache[name].Add(data);
                    handle(data);
                }, isURL, scale, shapeCorrection);
            }
            else
            {
                if (ActiveJob.IsRunning) Timing.KillCoroutines(ActiveJob);
                ActiveJob = Timing.RunCoroutine(LoopCache(handle, name));
            }
        }

        private static IEnumerator<float> LoopCache(Action<string> handle, string name)
        {
            foreach (var s in Images.Singleton.ImageCache[name])
            {
                handle(s);
                yield return Timing.WaitForSeconds(0.1f);
            }
        }
        
        private static List<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid()).ToList();
        }

        private static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        private static List<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count).ToList();
        }
    }
}