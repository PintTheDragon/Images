using System;
using System.Drawing;
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
                
                return new Bitmap(ms);
            }
        }

        private static Bitmap GetBitmapFromFile(string path)
        {
            if (!File.Exists(path)) return null;
            return new Bitmap(path);
        }

        private static string FileToText(string path, float scale = 0f, bool shapeCorrection = true)
        {
            var file = GetBitmapFromFile(path);
            if (file == null) return null;

            return BitmapToText(file, scale, shapeCorrection);
        }

        private static string URLToText(string url, float scale = 0f, bool shapeCorrection = true)
        {
            var file = GetBitmapFromURL(url);
            if (file == null) return null;

            return BitmapToText(file, scale, shapeCorrection);
        }

        public static string LocationToText(string loc, bool isURL = false, float scale = 0f, bool shapeCorrection = true)
        {
            return isURL ? URLToText(loc, scale, shapeCorrection) : FileToText(loc, scale, shapeCorrection);
        }

        public static string BitmapToText(Bitmap bitmap, float scale = 0f, bool shapeCorrection = true)
        {
            if(bitmap.Height * bitmap.Width > 1000) throw new Exception("The image was too large. Please use an image with less that 1,000 pixels (you shouldn't have an image with 40,000 pixels anyway).");

            var size = Convert.ToInt32(scale == 0f ? Math.Floor((-.47*(((bitmap.Width+bitmap.Height)/2 > 60 ? 45 : (bitmap.Width+bitmap.Height)/2)))+28.72) : scale);
            if(shapeCorrection) bitmap = new Bitmap(bitmap, new Size(Convert.ToInt32(bitmap.Width*(1+.03*size)), bitmap.Height));
            var text = "<size="+size+"%>";

            Color pastPixel = new Color();
            
            //I need to figure out how to use bitmap data, but GetPixel is fine for now
            for (var i = 0; i < bitmap.Height; i++)
            {
                for (var j = 0; j < bitmap.Width; j++)
                {
                    Color pixel = bitmap.GetPixel(j, i);

                    var colorString = "#" + pixel.R.ToString("X2") + pixel.G.ToString("X2") + pixel.B.ToString("X2") + pixel.A.ToString("X2");

                    var add = "";
                    
                    if(!pixel.Equals(pastPixel)){
                        text+=((i == 0 && j == 0) ? "" : "</color>")+"<color="+colorString+">█";
                    }
                    else{
                        text+="█";
                        add = "</color>";
                    }
                    
                    pastPixel = pixel;

                    if (j == bitmap.Width - 1) text += add+"\\n";
                }
            }
            
            if(text.Length > 32000) throw new Exception("Output text is too large. Please use a smaller image.");

            return text + "</color></size>";
        }
    }
}