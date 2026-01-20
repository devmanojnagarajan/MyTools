using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MyTools.Services
{
    public class CreateSurface
    {
        public static Face GetFaceFromFilledRegion(FilledRegion filledRegion, double offsetDistance)
        {
            Debug.WriteLine($"    [CreateSurface] GetFaceFromFilledRegion START - offsetDistance: {offsetDistance}");

            IList<CurveLoop> currentLoop = filledRegion.GetBoundaries();
            if (currentLoop.Count == 0)
            {
                Debug.WriteLine("    [CreateSurface] ERROR: Boundary Loop is not valid (count=0)");
                return null;
            }
            Debug.WriteLine($"    [CreateSurface] Found {currentLoop.Count} boundary loops");

            Plane regionPlane = currentLoop[0].GetPlane();
            XYZ normal = regionPlane.Normal;
            Debug.WriteLine($"    [CreateSurface] Region plane normal: ({normal.X:F3}, {normal.Y:F3}, {normal.Z:F3})");

            Transform moveBack = Transform.CreateTranslation(normal.Multiply(-offsetDistance));

            IList<CurveLoop> startProfile = new List<CurveLoop>();

            foreach (CurveLoop loop in currentLoop)
            {
                startProfile.Add(CurveLoop.CreateViaTransform(loop, moveBack));
            }
            Debug.WriteLine($"    [CreateSurface] Created {startProfile.Count} transformed curve loops");

            double extrusionDist = Math.Abs(0.1 * offsetDistance);
            if (extrusionDist < 0.001)
            {
                Debug.WriteLine($"    [CreateSurface] extrusionDist too small ({extrusionDist}), using minimum 0.001");
                extrusionDist = 0.001; // Minimum extrusion distance
            }
            Debug.WriteLine($"    [CreateSurface] Creating extrusion with distance: {extrusionDist}");

            Solid tempSolid = GeometryCreationUtilities.CreateExtrusionGeometry(startProfile, normal, extrusionDist);
            Debug.WriteLine($"    [CreateSurface] Solid created with {tempSolid.Faces.Size} faces");

            Face resultFace = tempSolid.Faces.get_Item(0) as Face;
            Debug.WriteLine($"    [CreateSurface] GetFaceFromFilledRegion END - Face: {(resultFace != null ? "OK" : "NULL")}");

            return resultFace;
        }
    }
}
