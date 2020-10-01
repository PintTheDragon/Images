using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace Images
{
    public static class API
    {
        private static Bitmap GetBitmapFromURL(string url)
        {
            var client = new WebClient();
            var stream = client.OpenRead(url);

            Bitmap bitmap = null;
            if (stream != null)
            {
                bitmap = new Bitmap(stream);
                
                stream.Flush();
                stream.Close();
            }
            
            client.Dispose();

            return bitmap;
        }

        private static Bitmap GetBitmapFromFile(string path)
        {
            if (!File.Exists(path)) return null;
            return new Bitmap(path);
        }

        private static string FileToText(string path)
        {
            var file = GetBitmapFromFile(path);
            if (file == null) return null;

            return BitmapToText(file);
        }

        private static string URLToText(string url)
        {
            var file = GetBitmapFromURL(url);
            if (file == null) return null;

            return BitmapToText(file);
        }

        public static string LocationToText(string loc, bool isURL = false)
        {
            return isURL ? URLToText(loc) : FileToText(loc);
        }

        public static string BitmapToText(Bitmap bitmap, float scale = 0f)
        {
            var size = Convert.ToInt32(scale == 0f ? Math.Floor((-.47*(((bitmap.Width+bitmap.Height)/2 > 60 ? 45 : (bitmap.Width+bitmap.Height)/2)))+28.72) : scale);
            bitmap = new Bitmap(bitmap, new Size(Convert.ToInt32(bitmap.Width*(1+.03*size)), bitmap.Height));
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
                        text+=(i != 0 ? "</color>" : "")+"<color="+colorString+">█";
                    }
                    else{
                        text+="█";
                        add = "</color>";
                    }
                    
                    pastPixel = pixel;

                    if (j == bitmap.Width - 1) text += add+"\n";
                }
            }

            return text + "</color></size>";
        }
    }
}