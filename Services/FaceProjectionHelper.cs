using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MyTools.Services
{
    public static class FaceProjectionHelper
    {
        /// <summary>
        /// Checks if a point is inside the boundary defined by CurveLoops (2D check, ignores Z)
        /// Uses ray casting algorithm for point-in-polygon test
        /// </summary>
        public static bool IsPointInsideBoundary(IList<CurveLoop> boundaries, XYZ point)
        {
            if (boundaries == null || boundaries.Count == 0 || point == null)
            {
                Debug.WriteLine($"      [FaceProjectionHelper] IsPointInsideBoundary - boundaries or point is null");
                return false;
            }

            // Get all vertices from the outer boundary (first curve loop)
            CurveLoop outerLoop = boundaries[0];
            List<XYZ> polygonPoints = new List<XYZ>();

            foreach (Curve curve in outerLoop)
            {
                polygonPoints.Add(curve.GetEndPoint(0));
            }

            if (polygonPoints.Count < 3)
            {
                Debug.WriteLine($"      [FaceProjectionHelper] Not enough points for polygon: {polygonPoints.Count}");
                return false;
            }

            // Use ray casting algorithm (2D - only X and Y)
            bool inside = IsPointInPolygon2D(polygonPoints, point.X, point.Y);

            if (inside)
            {
                Debug.WriteLine($"      [FaceProjectionHelper] INSIDE: Point ({point.X:F2}, {point.Y:F2}) is inside boundary");
            }
            else
            {
                Debug.WriteLine($"      [FaceProjectionHelper] OUTSIDE: Point ({point.X:F2}, {point.Y:F2}) is outside boundary");
            }

            return inside;
        }

        /// <summary>
        /// Ray casting algorithm for point-in-polygon test (2D)
        /// </summary>
        private static bool IsPointInPolygon2D(List<XYZ> polygon, double testX, double testY)
        {
            int n = polygon.Count;
            bool inside = false;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                double xi = polygon[i].X;
                double yi = polygon[i].Y;
                double xj = polygon[j].X;
                double yj = polygon[j].Y;

                // Check if point is on the same Y range as edge
                bool intersect = ((yi > testY) != (yj > testY)) &&
                                 (testX < (xj - xi) * (testY - yi) / (yj - yi) + xi);

                if (intersect)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        // Keep old method for backward compatibility but mark as obsolete
        [Obsolete("Use IsPointInsideBoundary instead")]
        public static bool IsPointOnFace(Face face, XYZ point)
        {
            return false;
        }
    }
}
