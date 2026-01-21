using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Diagnostics;

namespace MyTools.Services
{
    public class CreateSurface
    {
        /// <summary>
        /// Gets the boundary curves of a FilledRegion as a list of CurveLoops
        /// </summary>
        public static IList<CurveLoop> GetBoundaryLoops(FilledRegion filledRegion)
        {
            Debug.WriteLine($"    [CreateSurface] GetBoundaryLoops START");

            IList<CurveLoop> boundaries = filledRegion.GetBoundaries();

            if (boundaries == null || boundaries.Count == 0)
            {
                Debug.WriteLine("    [CreateSurface] ERROR: No boundaries found");
                return null;
            }

            Debug.WriteLine($"    [CreateSurface] Found {boundaries.Count} boundary loop(s)");

            // Log info about the first boundary
            if (boundaries.Count > 0)
            {
                CurveLoop firstLoop = boundaries[0];
                Plane plane = firstLoop.GetPlane();
                if (plane != null)
                {
                    Debug.WriteLine($"    [CreateSurface] Boundary plane origin: ({plane.Origin.X:F2}, {plane.Origin.Y:F2}, {plane.Origin.Z:F2})");
                    Debug.WriteLine($"    [CreateSurface] Boundary plane normal: ({plane.Normal.X:F2}, {plane.Normal.Y:F2}, {plane.Normal.Z:F2})");
                }

                int curveCount = 0;
                foreach (Curve c in firstLoop)
                {
                    curveCount++;
                }
                Debug.WriteLine($"    [CreateSurface] First loop has {curveCount} curves");
            }

            Debug.WriteLine($"    [CreateSurface] GetBoundaryLoops END");
            return boundaries;
        }
    }
}
