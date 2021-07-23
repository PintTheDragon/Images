using System;
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
        internal static HandleCommandObject HandleCommand(ArraySegment<string> arguments, ICommandSender sender, out string response, bool doDuration, string name, string perm)
        {
            if (!Images.Singleton.CacheReady)
            {
                response = "The cache is not ready yet. Please try again in a few seconds.";
                return null;
            }
            
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

            if (image.ContainsKey("scale") && image["scale"].Trim().ToLower() != "auto" && !int.TryParse(image["scale"].Trim().ToLower(), out scale))
            {
                response = "The scale parameter for this image is invalid. Only use integers or \"auto\".";
                return null;
            }

            var fps = 10;

            if (image.ContainsKey("fps") && image["fps"].Trim().ToLower() != "auto" && !int.TryParse(image["fps"].Trim().ToLower(), out fps))
            {
                response = "The fps parameter for this image is invalid. Only use integers.";
                return null;
            }

            var compress = true;

            if (image.ContainsKey("compress") && !bool.TryParse(image["compress"].Trim().ToLower(), out compress))
            {
                response = "The compress parameter for this image is invalid. Only use booleans";
                return null;
            }

            response = "Error";

            return new HandleCommandObject(imageList[0], duration, scale, fps, compress);
        }

        internal static CoroutineHandle LocationToText(string loc, Action<string> handle, string name, bool isURL = false, float scale = 0f, bool shapeCorrection = true, float waitTime = .1f, bool compress = true)
        {
            var cacheName = name + (shapeCorrection ? "y" : "n");
            
            if (Images.Singleton.ImageCache.Count > Images.Singleton.Config.CacheSize) Images.Singleton.ImageCache.Remove(Images.Singleton.ImageCache.Keys.First());

            CoroutineHandle coroutine;

            if (!Images.Singleton.ImageCache.ContainsKey(cacheName) || (Images.Singleton.ImageCache.ContainsKey(cacheName) && !Images.Singleton.ImageCache[cacheName].Complete))
            {
                Images.Singleton.ImageCache[cacheName] = new CachedImage();
                
                coroutine = API.LocationToText(loc, data =>
                {
                    if (!data.Last)
                    {
                        Images.Singleton.ImageCache[cacheName].Frames.Add(data.Data);
                        handle(data.Data);
                    }
                    else
                    {
                        Images.Singleton.ImageCache[cacheName].Complete = true;
                    }
                }, isURL, scale, shapeCorrection, waitTime, compress);
            }
            else
            {
                coroutine = Timing.RunCoroutine(LoopCache(handle, cacheName, waitTime));
            }
            
            Images.Singleton.Coroutines.Add(coroutine);

            return coroutine;
        }

        private static IEnumerator<float> LoopCache(Action<string> handle, string name, float waitTime = .1f)
        {
            foreach (var s in Images.Singleton.ImageCache[name].Frames)
            {
                handle(s);
                if(waitTime != 0f) yield return Timing.WaitForSeconds(waitTime);
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