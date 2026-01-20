using Autodesk.Revit.DB;
using MyTools.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTools
{
    public class CheckClash
    {
        public static List<Element> getClashingElements(Solid filledRegionVolume, List<Element> elementsInView)
        {
            List<Element> clashingList = new List<Element>();

            foreach (Element el in elementsInView)
            {
                // Get the accurate Geometry Center
                XYZ geoCenter = GeometryCenterService.GetTrueGeometryCenter(el);

                // Check if the point is inside your solid volume
                if (geoCenter != null && filledRegionVolume.Contains(geoCenter))
                {
                    clashingList.Add(el);
                }
            }
            return clashingList;

        }

    }
}
