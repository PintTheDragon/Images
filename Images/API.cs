using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using Exiled.API.Features;

namespace Images
{
    public static class API
    {
        private static Bitmap GetBitmapFromURL(string url)
        {
            using (var ms = new MemoryStream())
            {
                WebRequest.Create(url)?.GetResponse()?.GetResponseStream()?.CopyTo(ms);
                ms.Position = 0;

                Bitmap bitmap = new Bitmap(ms);
                
                ms.Flush();
                ms.Dispose();
                
                return bitmap;
            }
        }

        private static Bitmap GetBitmapFromFile(string path)
        {
            if (!File.Exists(path)) return null;
            return new Bitmap(path);
        }

        private static void FileToText(string path, Action<string> handle, float scale = 0f, bool shapeCorrection = true)
        {
            var file = GetBitmapFromFile(path);
            if (file == null) return;

            BitmapToText(file, handle, scale, shapeCorrection);
        }

        private static void URLToText(string url, Action<string> handle, float scale = 0f, bool shapeCorrection = true)
        {
            var file = GetBitmapFromURL(url);
            if (file == null) return;

            BitmapToText(file, handle, scale, shapeCorrection);
        }

        public static void LocationToText(string loc, Action<string> handle, bool isURL = false, float scale = 0f, bool shapeCorrection = true)
        {
            if (isURL)
            {
                URLToText(loc, handle, scale, shapeCorrection);
            }
            else
            {
                FileToText(loc, handle, scale, shapeCorrection);
            }
        }

        public static void BitmapToText(Bitmap bitmap, Action<string> handle, float scale = 0f, bool shapeCorrection = true)
        {
            if(bitmap.Height * bitmap.Width > 1000) throw new Exception("The image was too large. Please use an image with less that 1,000 pixels (you shouldn't have an image with 40,000 pixels anyway).");

            var size = Convert.ToInt32(scale == 0f ? Math.Floor((-.47*(((bitmap.Width+bitmap.Height)/2 > 60 ? 45 : (bitmap.Width+bitmap.Height)/2)))+28.72) : scale);
            if(shapeCorrection) bitmap = new Bitmap(bitmap, new Size(Convert.ToInt32(bitmap.Width*(1+.03*size)), bitmap.Height));

            for (var index = 0; index < bitmap.GetFrameCount(FrameDimension.Time); index++)
            {
                bitmap.SelectActiveFrame(FrameDimension.Time, index);
                
                var text = "<size=" + size + "%>";

                Color pastPixel = new Color();

                //I need to figure out how to use bitmap data, but GetPixel is fine for now
                for (var i = 0; i < bitmap.Height; i++)
                {
                    for (var j = 0; j < bitmap.Width; j++)
                    {
                        Color pixel = bitmap.GetPixel(j, i);

                        var colorString = "#" + pixel.R.ToString("X2") + pixel.G.ToString("X2") +
                                          pixel.B.ToString("X2") + pixel.A.ToString("X2");

                        var add = "";

                        if (!pixel.Equals(pastPixel))
                        {
                            text += ((i == 0 && j == 0) ? "" : "</color>") + "<color=" + colorString + ">█";
                        }
                        else
                        {
                            text += "█";
                            add = "</color>";
                        }

                        pastPixel = pixel;

                        if (j == bitmap.Width - 1) text += add + "\\n";
                    }
                }
                
                text+="</color></size>";

                if (text.Length > 32000) throw new Exception("Output text is too large. Please use a smaller image.");

                handle(text);
            }

            bitmap.Dispose();
        }
    }
}