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
        public Bitmap image { get; set; }
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
        }

        class Perks
        {
            public List<PerkInfo> perks { get; set; }
        }

    }
    

}
