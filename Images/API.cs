using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Exiled.API.Features;
using MEC;

namespace Images
{
    public static class API
    {
        private static Image GetBitmapFromURL(string url)
        {
            var ms = new MemoryStream();
            
            var stream = WebRequest.Create(url)?.GetResponse()?.GetResponseStream();
            if (stream == null) return null;
                
            stream.CopyTo(ms);
            ms.Position = 0;

            Image image = Image.FromStream(ms);
                
            stream.Flush();
            stream.Dispose();
                
            return image;
            }

        private static Image GetBitmapFromFile(string path)
        {
            if (!File.Exists(path)) return null;

            return Image.FromFile(path);
        }

        private static CoroutineHandle FileToText(string path, Action<FrameData> handle, float scale = 0f, bool shapeCorrection = true, float waitTime = .1f, bool compress = true)
        {
            var file = GetBitmapFromFile(path);
            if (file == null) return new CoroutineHandle();

            return BitmapToText(file, handle, scale, shapeCorrection, waitTime, compress);
        }

        private static CoroutineHandle URLToText(string url, Action<FrameData> handle, float scale = 0f, bool shapeCorrection = true, float waitTime = .1f, bool compress = true)
        {
            var file = GetBitmapFromURL(url);
            if (file == null) return new CoroutineHandle();

            return BitmapToText(file, handle, scale, shapeCorrection, waitTime, compress);
        }

        /// <summary>
        /// Converts an image from a File Path or URL to some text.
        /// </summary>
        /// <param name="loc">The File Path/URL of the image.</param>
        /// <param name="handle">An <see cref="Action<FrameData>"/> that will be ran for each frame of the image.</param>
        /// <param name="isURL">Whether the location is a URL.</param>
        /// <param name="scale">The <see cref="float"/> that determines the scale. Leave at default to automatically calculate scale.</param>
        /// <param name="shapeCorrection">Whether the shape of the image should be automatically corrected.</param>
        /// <param name="waitTime">How long should be waited after every frame in an image.</param>
        /// <param name="compress">Should compression be applied to the image?</param>
        public static CoroutineHandle LocationToText(string loc, Action<FrameData> handle, bool isURL = false, float scale = 0f, bool shapeCorrection = true, float waitTime = .1f, bool compress = true)
        {
            if (isURL)
            {
                return URLToText(loc, handle, scale, shapeCorrection, waitTime, compress);
            }
            else
            {
                return FileToText(loc, handle, scale, shapeCorrection, waitTime, compress);
            }
        }

        /// <summary>
        /// Converts an image to some text.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> that will be converted to text.</param>
        /// <param name="handle">An <see cref="Action<FrameData>"/> that will be ran for each frame of the image.</param>
        /// <param name="scale">The <see cref="float"/> that determines the scale. Leave at default to automatically calculate scale.</param>
        /// <param name="shapeCorrection">Whether or not the shape of the image should be automatically corrected.</param>
        /// <param name="waitTime">How long should be waited after every frame in an image.</param>
        /// <param name="compress">Should compression be applied to the image?</param>
        public static CoroutineHandle BitmapToText(Image bitmap, Action<FrameData> handle, float scale = 0f, bool shapeCorrection = true, float waitTime = .1f, bool compress = true)
        {
            return Timing.RunCoroutine(_BitmapToText(bitmap, handle, scale, shapeCorrection, waitTime, compress));
        }
        
        private static IEnumerator<float> _BitmapToText(Image image, Action<FrameData> handle, float scale = 0f, bool shapeCorrection = true, float waitTime = .1f, bool compress = true)
        {
            if (image == null) yield break;

            var wait = TimeSpan.FromSeconds(waitTime);

            var size = 0f;

            var dim = new FrameDimension(image.FrameDimensionsList[0]);
            var frames = image.GetFrameCount(dim);

            var fails = 0;
            
            for (var index = 0; index < frames; index++)
            {
                var time = DateTime.Now;
                
                image.SelectActiveFrame(dim, index);
                
                if(image.Size.Height * image.Size.Width > 10000) throw new Exception("The image was too large. Please use an image with less that 10,000 pixels. Your image doesn't need to be more than 100x100.");

                if (size == 0f)
                {
                    size = Convert.ToInt32(scale == 0f ? Math.Floor((-.47*(((image.Size.Width+image.Size.Height)/2 > 60 ? 45 : (image.Width+image.Height)/2)))+28.72) : scale);
                }

                Bitmap bitmap;
                if(shapeCorrection) bitmap = new Bitmap(image, new Size(Convert.ToInt32(image.Size.Width*(1+.03*size)), image.Size.Height));
                else bitmap = new Bitmap(image);

                var text = "<size=" + size + "%>";

                var pastPixel = new Color();
                
                var threshold = 0f;

                while ((System.Text.Encoding.Unicode.GetByteCount(text) > 32768  || text == "<size=" + size + "%>") && threshold < 5f)
                {
                    text = "<size=" + size + "%>";
                    
                    //I need to figure out how to use bitmap data, but GetPixel is fine for now
                    for (var i = 0; i < bitmap.Height; i++)
                    {
                        for (var j = 0; j < bitmap.Width; j++)
                        {
                            var pixel = bitmap.GetPixel(j, i);

                            var colorString = "#" + pixel.R.ToString("X2") + pixel.G.ToString("X2") + pixel.B.ToString("X2") + pixel.A.ToString("X2");

                            if (!pixel.Equals(pastPixel))
                            {
                                if (threshold == 0f || (i == 0 && j == 0)) text += ((i == 0 && j == 0) ? "" : "</color>") + "<color=" + colorString + ">█";
                                else
                                {
                                    var d1 = Math.Abs(pixel.GetHue() - pastPixel.GetHue());
                                    var d2 = Math.Abs(pixel.GetSaturation() - pastPixel.GetSaturation());
                                    var d3 = Math.Abs(pixel.GetBrightness() - pastPixel.GetBrightness());
                                    
                                    var diff = (((d1 > 180 ? 360 - d1 : d1) * .755f) + (d2 * 2f) + (d3 * .7f))/3;

                                    if (compress && diff > threshold) text += ((i == 0 && j == 0) ? "" : "</color>") + "<color=" + colorString + ">█";
                                    else
                                    {
                                        pixel = pastPixel;
                                        text += "█";
                                    }
                                }
                            }
                            else
                            {
                                text += "█";
                            }

                            pastPixel = pixel;

                            if (j == bitmap.Width - 1) text += "\\n";
                        }
                    }

                    if (!text.EndsWith("</color>\\n") && !text.EndsWith("</color>")) text += "</color>";

                    text += "</size>";

                    threshold += .5f;

                    yield return Timing.WaitForOneFrame;
                }

                if (System.Text.Encoding.Unicode.GetByteCount(text) > 32768)
                {
                    fails++;
                    yield return Timing.WaitForOneFrame;
                    continue;
                }
                
                handle(new FrameData(text));

                if (waitTime != 0f)
                {
                    var diff = DateTime.Now - time;
                    if(diff < wait) yield return Timing.WaitForSeconds((float) (wait-diff).TotalSeconds);
                    else yield return Timing.WaitForOneFrame;
                }
                else yield return Timing.WaitForOneFrame;
            }

            image.Dispose();
            
            handle(new FrameData(null) {Last = true});
            
            if(frames == 1 && fails > 0) throw new Exception("The image is too large to display.");
            if(fails > 0) throw new Exception(fails+" frames have been dropped while attempting to display this image.");
        }
    }
}