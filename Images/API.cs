using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
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

        private static CoroutineHandle FileToText(string path, Action<string> handle, float scale = 0f, bool shapeCorrection = true)
        {
            var file = GetBitmapFromFile(path);
            if (file == null) return new CoroutineHandle();

            return BitmapToText(file, handle, scale, shapeCorrection);
        }

        private static CoroutineHandle URLToText(string url, Action<string> handle, float scale = 0f, bool shapeCorrection = true)
        {
            var file = GetBitmapFromURL(url);
            if (file == null) return new CoroutineHandle();

            return BitmapToText(file, handle, scale, shapeCorrection);
        }

        public static CoroutineHandle LocationToText(string loc, Action<string> handle, bool isURL = false, float scale = 0f, bool shapeCorrection = true)
        {
            if (isURL)
            {
                return URLToText(loc, handle, scale, shapeCorrection);
            }
            else
            {
                return FileToText(loc, handle, scale, shapeCorrection);
            }
        }

        public static CoroutineHandle BitmapToText(Image bitmap, Action<string> handle, float scale = 0f, bool shapeCorrection = true)
        {
            return Timing.RunCoroutine(_BitmapToText(bitmap, handle, scale, shapeCorrection));
        }

        private static IEnumerator<float> _BitmapToText(Image image, Action<string> handle, float scale = 0f, bool shapeCorrection = true, float waitTime = 1f)
        {
            if (image == null) yield break;
            
            var size = 0f;

            var dim = new FrameDimension(image.FrameDimensionsList[0]);

            for (var index = 0; index < image.GetFrameCount(dim); index++)
            {
                image.SelectActiveFrame(dim, index);

                if (size == 0f)
                {
                    if(image.Size.Height * image.Size.Width > 1000) throw new Exception("The image was too large. Please use an image with less that 1,000 pixels (you shouldn't have an image with 40,000 pixels anyway).");
                    size = Convert.ToInt32(scale == 0f ? Math.Floor((-.47*(((image.Size.Width+image.Size.Height)/2 > 60 ? 45 : (image.Width+image.Height)/2)))+28.72) : scale);
                }

                Bitmap bitmap;
                if(shapeCorrection) bitmap = new Bitmap(image, new Size(Convert.ToInt32(image.Size.Width*(1+.03*size)), image.Size.Height));
                else bitmap = new Bitmap(image);
                
                var text = "<size=" + size + "%>";

                Color pastPixel = new Color();

                //I need to figure out how to use bitmap data, but GetPixel is fine for now
                for (var i = 0; i < bitmap.Height; i++)
                {
                    for (var j = 0; j < bitmap.Width; j++)
                    {
                        Color pixel = bitmap.GetPixel(j, i);

                        var colorString = "#" + pixel.R.ToString("X2") + pixel.G.ToString("X2") + pixel.B.ToString("X2") + pixel.A.ToString("X2");
                        
                        if (!pixel.Equals(pastPixel))
                        {
                            text += ((i == 0 && j == 0) ? "" : "</color>") + "<color=" + colorString + ">█";
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
                
                text+="</size>";

                if (text.Length > 32000) throw new Exception("Output text is too large. Please use a smaller image.");

                handle(text);

                yield return Timing.WaitForSeconds(.1f);
            }

            image.Dispose();
        }
    }
}