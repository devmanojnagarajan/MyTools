using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace MyTools.Services
{
    /// <summary>
    /// Detects elements that fall within region boundaries.
    /// </summary>
    public static class CheckClash
    {
        /// <summary>
        /// Returns elements whose geometric centers are inside the given region boundaries.
        /// </summary>
        public static List<Element> GetClashingElements(IList<CurveLoop> regionBoundaries, List<Element> elementsInView)
        {
            List<Element> clashingList = new List<Element>();

            if (regionBoundaries == null || regionBoundaries.Count == 0)
                return clashingList;

            foreach (Element el in elementsInView)
            {
                XYZ? geoCenter = GeometryCenter.GetGeometryCenter(el);

                if (geoCenter == null)
                    continue;

                if (FaceProjectionHelper.IsPointInsideBoundary(regionBoundaries, geoCenter))
                    clashingList.Add(el);
            }

            return clashingList;
        }
    }
}
