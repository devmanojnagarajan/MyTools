using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Diagnostics;
using MyTools.Model;

namespace MyTools.Services
{
    public class CheckClash
    {
        public static List<Element> getClashingElements(Face targetFace, List<Element> elementsInView)
        {
            Debug.WriteLine($"    [CheckClash] getClashingElements START - checking {elementsInView.Count} elements");

            List<Element> clashingList = new List<Element>();

            if (targetFace == null)
            {
                Debug.WriteLine("    [CheckClash] ERROR: targetFace is null, returning empty list");
                return clashingList;
            }

            int processedCount = 0;
            int nullCenterCount = 0;

            foreach (Element el in elementsInView)
            {
                processedCount++;
                XYZ geoCenter = GeometryCenter.GetGeometryCenter(el);

                if (geoCenter == null)
                {
                    nullCenterCount++;
                    continue;
                }

                if (FaceProjectionHelper.IsPointOnFace(targetFace, geoCenter))
                {
                    clashingList.Add(el);
                    Debug.WriteLine($"    [CheckClash] CLASH: Element {el.Id} ({el.Category?.Name ?? "No Category"}) at ({geoCenter.X:F2}, {geoCenter.Y:F2}, {geoCenter.Z:F2})");
                }
            }

            Debug.WriteLine($"    [CheckClash] getClashingElements END - Processed: {processedCount}, NullCenters: {nullCenterCount}, Clashes: {clashingList.Count}");
            return clashingList;
        }
    }
}