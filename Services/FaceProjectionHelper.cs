using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace MyTools.Services
{
    /// <summary>
    /// Helper class for 2D point-in-polygon operations.
    /// </summary>
    public static class FaceProjectionHelper
    {
        /// <summary>
        /// Checks if a point is inside the boundary defined by CurveLoops (2D check, ignores Z).
        /// Uses ray casting algorithm for point-in-polygon test.
        /// </summary>
        public static bool IsPointInsideBoundary(IList<CurveLoop> boundaries, XYZ point)
        {
            if (boundaries == null || boundaries.Count == 0 || point == null)
                return false;

            CurveLoop outerLoop = boundaries[0];
            List<XYZ> polygonPoints = new List<XYZ>();

            foreach (Curve curve in outerLoop)
            {
                polygonPoints.Add(curve.GetEndPoint(0));
            }

            if (polygonPoints.Count < 3)
                return false;

            return IsPointInPolygon2D(polygonPoints, point.X, point.Y);
        }

        /// <summary>
        /// Ray casting algorithm for point-in-polygon test (2D).
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

                bool intersect = ((yi > testY) != (yj > testY)) &&
                                 (testX < (xj - xi) * (testY - yi) / (yj - yi) + xi);

                if (intersect)
                    inside = !inside;
            }

            return inside;
        }
    }
}
