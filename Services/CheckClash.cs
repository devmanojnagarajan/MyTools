using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Diagnostics;
using MyTools.Model;

namespace MyTools.Services
{
    public class CheckClash
    {
        public static List<Element> GetClashingElements(IList<CurveLoop> regionBoundaries, List<Element> elementsInView)
        {
            Debug.WriteLine($"    [CheckClash] GetClashingElements START - checking {elementsInView.Count} elements");

            List<Element> clashingList = new List<Element>();

            if (regionBoundaries == null || regionBoundaries.Count == 0)
            {
                Debug.WriteLine("    [CheckClash] ERROR: regionBoundaries is null or empty");
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

                if (FaceProjectionHelper.IsPointInsideBoundary(regionBoundaries, geoCenter))
                {
                    clashingList.Add(el);
                    Debug.WriteLine($"    [CheckClash] CLASH: Element {el.Id} ({el.Category?.Name ?? "No Category"}) at ({geoCenter.X:F2}, {geoCenter.Y:F2})");
                }
            }

            Debug.WriteLine($"    [CheckClash] GetClashingElements END - Processed: {processedCount}, NullCenters: {nullCenterCount}, Clashes: {clashingList.Count}");
            return clashingList;
        }
    }
}
