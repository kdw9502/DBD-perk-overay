using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DBD_perk
{
    public class PerkInfo
    {
        public string name { get; set; }
        public string fileName { get; set; }
        public string displayName { get; set; }
        public string desc { get; set; }
        public Mat image { get; set; }
        public Mat darkerImage { get; set; }
    }

    public static class PerkInfoLoader
    {
        public static List<PerkInfo> infos = new List<PerkInfo>();
        public static void load()
        {
            var jsonUtf8Bytes = File.ReadAllBytes("../../../resources/new_perk_info.txt");
            var readOnlySpan = new ReadOnlySpan<byte>(jsonUtf8Bytes);
            var perks = JsonSerializer.Deserialize<Perks>(readOnlySpan);
            infos = perks.perks;

            foreach(var info in infos)
            {
                var image = new Bitmap($"../../../resources/Perks/{info.fileName}.png");
                image = new Bitmap(image, new System.Drawing.Size(103, 103));
                Mat origin = OpenCvSharp.Extensions.BitmapConverter.ToMat(image);
                //Mat temp = new Mat();

                Cv2.CvtColor(origin, origin, ColorConversionCodes.BGR2GRAY);                

                info.image = origin;

                //origin.CopyTo(temp);
                //origin.ConvertTo(temp, origin.Type(), 0.35,0);
                //if (info.fileName == "noOneEscapesDeath")
                //    temp.SaveImage("noOne.png");

                //info.darkerImage = temp;       
            }
        }

        class Perks
        {
            public List<PerkInfo> perks { get; set; }
        }

    }
    

}
