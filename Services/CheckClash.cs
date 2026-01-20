using Autodesk.Revit.DB;
using MyTools.Services;
using System;
using System.Collections.Generic;

namespace MyTools.Services
{
    public class CheckClash
    {
        public static List<Element> getClashingElements(Face targetFace, List<Element> elementsInView)
        {
            List<Element> clashingList = new List<Element>();

            if (targetFace == null) return clashingList;

            foreach (Element el in elementsInView)
            {
                // 1. Get the accurate Geometry Center (Centroid)
                XYZ geoCenter = GeometryCenter.GetGeometryCenter(el);

                if (geoCenter != null)
                {
                    // 2. Project the center point onto the single surface/face
                    IntersectionResult result = targetFace.Project(geoCenter);

                    // 3. If result is not null, a projection was mathematically possible
                    // We check Distance to ensure it is "on" the surface within a tolerance
                    if (result != null && result.Distance < 0.001)
                    {
                        clashingList.Add(el);
                    }
                }
            }
            return clashingList;
        }
    }
}