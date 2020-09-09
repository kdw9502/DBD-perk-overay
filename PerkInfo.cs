using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DBD_perk
{
    public class PerkInfo: IEquatable<PerkInfo>
    {
        public string name { get; set; }
        public string fileName { get; set; }
        public string displayName { get; set; }
        public string desc { get; set; }
        public Mat image { get; set; }
        public double matchRate { get; set; }

        public bool Equals([AllowNull] PerkInfo other)
        {
            return fileName == other.fileName;
        }
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
                var image = new Bitmap($"../../../resources/perks_crop/{info.fileName}.png");
                image = new Bitmap(image, new System.Drawing.Size(51, 51));
                Mat origin = OpenCvSharp.Extensions.BitmapConverter.ToMat(image);
                //Mat temp = new Mat();

                Cv2.CvtColor(origin, origin, ColorConversionCodes.BGR2GRAY);                

                info.image = origin;
            }
        }

        class Perks
        {
            public List<PerkInfo> perks { get; set; }
        }

    }
    

}
