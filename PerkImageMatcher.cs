using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenCvSharp;

namespace DBD_perk
{
    public static class PerkImageMatcher
    {
        private static bool matched(Bitmap screenshot, Bitmap perk)
        {
            using (Mat screenMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(screenshot))            
            using (Mat perkMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(perk))
            using (Mat result = new Mat())
            {                
                Cv2.MatchTemplate(screenMat, perkMat, result, TemplateMatchModes.CCoeffNormed);

                OpenCvSharp.Point minloc, maxloc;
                double minval, maxval;
                Cv2.MinMaxLoc(result, out minval, out maxval, out minloc, out maxloc);

                var threshold = 0.8;

                return maxval >= threshold;               

            }
        }       

    }
}
